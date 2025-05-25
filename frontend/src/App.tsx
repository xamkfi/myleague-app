import { useState } from 'react'
import { Container, Row, Col } from 'react-bootstrap'
import 'bootstrap/dist/css/bootstrap.min.css'
import './App.css'
import WaitingRoom from './components/WaitingRoom'
import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import ChatRoom from './components/ChatRoom'
import MessageInput from './components/MessageInput'

function App() {
  const [connection, setConnection] = useState<HubConnection>()
  const [messages, setMessages] = useState([])

  // Method used for joining chat room
  const joinChatRoom = async (userName: string, chatRoom: string) => {
    try{
      // Initiate a connection
      const conn = new HubConnectionBuilder()
                    .withUrl("http://localhost:5111/chat")
                    .configureLogging(LogLevel.Information)
                    .build()

      // Set up handlers
      // Backend tells frontend to run these methods when called
      conn.on("JoinSpecificChatRoom", (userName, msg) => {
        setMessages(messages => [...messages, {userName, msg}])
        console.log("message: ", msg)
      })

      conn.on("ReceiveSpecificMessage", (userName, msg) => {
        setMessages(messages => [...messages, {userName, msg}])
        console.log("", msg)
      })

      await conn.start()
      // Frontend tells what method to run in backend
      await conn.invoke("JoinSpecificChatRoom", {userName, chatRoom})

      setConnection(conn)

    }catch(err){
      console.log("error occured...: ", err)
    }
  }

  // Method used when sending a message
  const sendMessage = async (msg: string) => {
    await connection?.invoke("SendMessage", msg)
    console.log(msg)
  }

  return (
    <div>
      <main>
        <Container>
          <Row className='px-5 my-5'>
            <Col sm='12'>
              <h1 className='font-weight-light'>Welcome to an Amazing chat app!</h1>
            </Col>
          </Row>
          {!connection ? (
            <WaitingRoom joinChatRoom={joinChatRoom}></WaitingRoom>
          ) : (
            <div>
              <ChatRoom sendMessage={sendMessage} messages={messages}></ChatRoom>
            </div>
          )}
        </Container>
      </main>
    </div>
  )
}

export default App
