package main

import (
	"github.com/gin-contrib/sessions"
	"github.com/gin-contrib/sessions/cookie"
	"github.com/gin-gonic/contrib/static"
	"github.com/gin-gonic/gin"
	"net/http"
)

var router *gin.Engine

func main() {
	gin.SetMode(gin.ReleaseMode)
	router = gin.Default()
	router.MaxMultipartMemory = 8 << 20
	router.Static("/files", "./files")
	router.Use(static.Serve("/assets/css", static.LocalFile("./templates/assets/css", true)))
	router.Use(static.Serve("/assets/images", static.LocalFile("./templates/assets/images", true)))
	router.Use(static.Serve("/assets/js", static.LocalFile("./templates/assets/js", true)))
	router.Use(static.Serve("/assets/fonts", static.LocalFile("./templates/assets/fonts", true)))
	router.LoadHTMLGlob("templates/*.html")
	store := cookie.NewStore([]byte("cmzjhobeielszohqnkethavecwxmyzuz"))
	router.Use(sessions.Sessions("session", store))
	initializeRoutes()
	router.Run("localhost:3716")

}

func render(c *gin.Context, data gin.H, templateName string) {
	switch c.Request.Header.Get("Accept") {
	case "application/json":
		c.JSON(http.StatusOK, data["payload"])
	case "application/xml":
		c.XML(http.StatusOK, data["payload"])
	default:
		c.HTML(http.StatusOK, templateName, data)
	}
}