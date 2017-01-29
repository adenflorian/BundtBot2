const express = require('express')
const app = express()
const port = 3000

const limit = 5
const offset = 5

var remaining = 5
var reset = getTimestamp() + 5

app.get('/', (request, response) => {
  var warning = "";
  if (remaining <= 0) {
    warning = "RATE LIMIT EXCEEDED! "
  }
  remaining--
  response.setHeader("X-RateLimit-Limit", limit)
  response.setHeader("X-RateLimit-Remaining", remaining)
  response.setHeader("X-RateLimit-Reset", reset)
  response.send(`${warning}Now: ${getTimestamp()} Limit: ${limit} | Remaining: ${remaining} | Reset: ${reset}`)
})

app.listen(port, (err) => {
  if (err) {
    return console.log('something bad happened', err)
  }

  console.log(`server is listening on ${port}`)
})

function getTimestamp() {
  return Math.floor(Date.now() / 1000)
}

function WaitThenReset(seconds) {
  setTimeout(function () {
    remaining = limit;
    reset = getTimestamp() + offset
    WaitThenReset(reset - getTimestamp());
  }, seconds * 1000)
}

WaitThenReset(reset - getTimestamp());