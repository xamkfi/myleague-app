import React, { useState } from 'react'
import { Button, Col, Form } from 'react-bootstrap'

export default function MessageInput({ sendMessage }) {
   const [message, setMessage] = useState<string>('')

   const onSubmitClick = (e: React.FormEvent<HTMLFormElement>) => {
      e.preventDefault()
      sendMessage(message)
      setMessage('')
   }

   return (
      <div >
         <hr></hr>
         <Form onSubmit={(e) => onSubmitClick(e)} className='d-flex'>
            <Form.Control 
               className='me-2' 
               placeholder='Type something...'
               onChange={(e) => setMessage(e.target.value)}></Form.Control>
            <Button type='submit'>Send</Button>
         </Form>
      </div>
   )
}