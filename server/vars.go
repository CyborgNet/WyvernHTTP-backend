package main

import "fmt"

var (
	userAgent = "Mozilla/5.0 (X11; U; Linux armv7l like Android; en-us) AppleWebKit/531.2+ (KHTML, like Gecko) Version/5.0 Safari/533.2+ Kindle/3.0+"

	dbHost = "localhost"
	dbPort = "3306"
	dbUsername = "wyvern"
	dbPassword = "Abc123^^"
	dbName = "wyvern"

	dbQuery = fmt.Sprintf("%s:%s@tcp(%s:%s)/%s", dbUsername, dbPassword, dbHost, dbPort, dbName)
)
