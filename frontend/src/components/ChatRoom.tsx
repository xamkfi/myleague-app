import { Col, Row } from "react-bootstrap"
import MessageContainer  from './MessageContainer'
import MessageInput from "./MessageInput"

export default function ChatRoom ({ messages, sendMessage}) {
   return (
   <Row className="px-5 py-5">
      <Col sm={10}>
      <h2>Chat Room</h2>
      </Col>
      <Col>
      
      </Col>
      <Row className="px-5 py-5">
         <Col sm={12}>
            <MessageContainer messages={messages}></MessageContainer>
            <MessageInput sendMessage={sendMessage}></MessageInput>
            
         </Col>
      </Row>
   </Row>
   )
}