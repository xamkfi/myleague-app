export default function MessageContainer ({ messages }) {

   return (
      <div>
         {
            messages.map((msg, index) => (
               <table>
                  <tr key={index}>
                     <td>{msg.userName} - {msg.msg}</td>
                  </tr>
               </table>
            ))
         }
      </div>
   )
}