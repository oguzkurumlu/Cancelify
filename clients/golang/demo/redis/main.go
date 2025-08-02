package main

import (
	"fmt"
	"time"

	cancelifyredis "github.com/oguzkuru/cancelify-go/redis"
)

func main() {
	// Initialize the Redis cancel manager
	manager, err := cancelifyredis.New("localhost:6379", "cancel-token:")
	if err != nil {
		panic(err)
	}
	defer manager.Close()

	jobID := "task-oguz-123"
	ctx := manager.GetContext(jobID)
	for {
		select {
		default:
			fmt.Print("Running\n")
			time.Sleep(1 * time.Second)
			break
		case <-ctx.Done():
			fmt.Print("Job cancelled\n")
			return
		}
	}
}
