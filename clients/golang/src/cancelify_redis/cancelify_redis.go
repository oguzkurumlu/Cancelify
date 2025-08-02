package cancelify_redis

import (
	"context"
	"strings"
	"sync"

	"github.com/redis/go-redis/v9"
)

type CancelifyRedis struct {
	rdb           *redis.Client
	pubsub        *redis.PubSub
	channelPrefix string
	ctx           context.Context
	cancelMap     map[string]context.CancelFunc
	mu            sync.Mutex
}

func New(redisAddr string, channelPrefix string) (*CancelifyRedis, error) {
	ctx := context.Background()
	rdb := redis.NewClient(&redis.Options{
		Addr: redisAddr,
	})

	pubsub := rdb.PSubscribe(ctx, channelPrefix+"*")
	if _, err := pubsub.Receive(ctx); err != nil {
		return nil, err
	}

	manager := &CancelifyRedis{
		rdb:           rdb,
		pubsub:        pubsub,
		channelPrefix: channelPrefix,
		ctx:           ctx,
		cancelMap:     make(map[string]context.CancelFunc),
	}

	go manager.listen()

	return manager, nil
}

func (m *CancelifyRedis) listen() {
	ch := m.pubsub.Channel()
	for msg := range ch {
		jobID := strings.TrimPrefix(msg.Channel, m.channelPrefix)

		m.mu.Lock()
		if cancel, ok := m.cancelMap[jobID]; ok {
			cancel()
			delete(m.cancelMap, jobID)
		}
		m.mu.Unlock()
	}
}

func (m *CancelifyRedis) GetContext(jobID string) context.Context {
	m.mu.Lock()
	defer m.mu.Unlock()

	if _, exists := m.cancelMap[jobID]; exists {
		ctx, _ := context.WithCancel(context.Background())
		return ctx // Already exists, return non-cancellable fallback
	}

	ctx, cancel := context.WithCancel(context.Background())
	m.cancelMap[jobID] = cancel
	return ctx
}

func (m *CancelifyRedis) Cancel(jobID string) error {
	return m.rdb.Publish(m.ctx, m.channelPrefix+jobID, "cancel").Err()
}

func (m *CancelifyRedis) Close() error {
	m.mu.Lock()
	defer m.mu.Unlock()

	for _, cancel := range m.cancelMap {
		cancel()
	}
	m.cancelMap = make(map[string]context.CancelFunc)

	if err := m.pubsub.Close(); err != nil {
		return err
	}
	return m.rdb.Close()
}
