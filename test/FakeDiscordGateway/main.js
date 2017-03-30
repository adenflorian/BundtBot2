var ws = require("nodejs-websocket")

var port = 8001;

var hello = {
    "op": 10,
    "d": {
        "heartbeat_interval": 1000
    }
}

// Scream server example: "hi" -> "HI!!!" 
var server = ws.createServer(function (conn) {
    console.log("New connection")
    conn.on("text", function (str) {
        console.log("Received " + str)
        conn.sendText(str.toUpperCase() + "!!!")
    })
    conn.on("close", function (code, reason) {
        console.log("Connection closed")
    })
    conn.on("error", function (err) {
        console.log("error: " + err)
    })
    conn.sendText(JSON.stringify(hello))
}).listen(port)

console.log("Listening on port " + port);