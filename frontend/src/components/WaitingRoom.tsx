import { useState } from "react"
import { Form, Row, Col, Button } from "react-bootstrap"

export default function ({ joinChatRoom }){
   const [userName, setUserName] = useState<string>("")
   const [chatRoom, setChatRoom] = useState<string>("")

   const onFormSubmit= (e) => {
      e.preventDefault()
      joinChatRoom(userName, chatRoom)
   }

   return (
      <Form onSubmit={(e) => onFormSubmit(e)}> 
         <Row className="px-5 py-5">
            <Col sm={12}>
               <Form.Group>
                  <Form.Control 
                     placeholder="UserName" 
                     onChange={(e) => setUserName(e.target.value)}></Form.Control>

                  <Form.Control 
                     placeholder="ChatRoom"
                     onChange={(e) => setChatRoom(e.target.value)}></Form.Control>
               </Form.Group>
            </Col>
            <Col>
               <hr />
               <Button variant="success" type="submit">Join</Button>
            </Col>
         </Row>
      </Form>
   )
}