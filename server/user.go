package main

import (
	"database/sql"
	"fmt"
	"github.com/gin-contrib/sessions"
	"github.com/gin-gonic/gin"
	_ "github.com/go-sql-driver/mysql"
	"net/http"
)

func performLogout(c *gin.Context) {
	session := sessions.Default(c)
	session.Set("username", "")
	session.Set("password", "")
	session.Save()
	c.Redirect(302, "/login")
}

func performLogin(c *gin.Context) {
	username := c.PostForm("username")
	password := c.PostForm("password")
	if isUserValid(username, password) {
		session := sessions.Default(c)
		session.Set("username", username)
		session.Set("password", password)
		session.Save()
		c.Redirect(302, "/dashboard")
	} else if !isUserExists(username) {
		c.HTML(http.StatusBadRequest, "login.html", gin.H{
			"ErrorTitle": "Wrong username",
			"ErrorMessage": "Username not exist",
		})
	} else {
		c.HTML(http.StatusBadRequest, "login.html", gin.H{
			"ErrorTitle": "Wrong Account",
			"ErrorMessage": "Account information isn't correct",
		})
	}
}

func isUserExists(username string) bool {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	query := fmt.Sprintf("SELECT EXISTS(SELECT * from user WHERE username=\"%s\")", username)
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

func isUserValid(username, password string) bool {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	if isUserExists(username) {
		query := fmt.Sprintf("SELECT password from user WHERE username=\"%s\"", username)
		rows := db.QueryRow(query)
		var pw string
		err = rows.Scan(&pw)
		if err != nil {
			fmt.Println(err)
		}
		if pw == password {
			return true
		} else {
			return false
		}
	} else {
		return false
	}
}

func getInterval() int {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	rows := db.QueryRow("SELECT interval_m from setting")
	var interval int
	err = rows.Scan(&interval)
	if err != nil {
		fmt.Println(err)
	}
	return interval
}

func getDeadAfter() int {
	db, err := sql.Open("mysql", dbQuery)
	if err != nil {
		fmt.Println(err)
	}
	defer db.Close()
	rows := db.QueryRow("SELECT dead_after from setting")
	var deadAfter int
	err = rows.Scan(&deadAfter)
	if err != nil {
		fmt.Println(err)
	}
	return deadAfter
}
