import { useEffect, useState, useRef } from 'react'
import './ErrorPopup.scss'
import CancelSVG from '../../assets/basicIcons/cancel.svg'
import CloseSVG from '../../assets/basicIcons/close.svg'
import type { ParsedError } from '../../types/common/errorTypes'

interface ErrorPopupProps {
   message: string | null,
}

function ErrorPopup({message}: ErrorPopupProps) {
   const [errors, setErrors] = useState<object | string>("")
   const [errorTitle, setErrorTitle] = useState<string>("")
   const [isShown, setIsShown] = useState<boolean>(false)
   const popUpDisplay = useRef<HTMLDivElement>(null)

   const handleCloseClick = () => {
      setIsShown(false)
   }

   useEffect(() => {
      if (message && Object.keys(message).length > 1){
         parseErrorMessage(message)
         setIsShown(true)
      }
   }, [message])

   // useEffect(() => {
   //    if (errors){
   //       console.log(errors)
   //    }
   // }, [errors])

   const parseErrorMessage = (msg: string) => {
      if (msg == null)
         return
      
      const raw = msg.replace(/^Error:\s*/, "")
      let tempMsg: ParsedError
      try {
         tempMsg = JSON.parse(raw)
      } catch {
         const start = raw.indexOf("{")
         const end = raw.lastIndexOf("}")
         if (start !== -1 && end !== -1 && end > start) {
            const jsonPart = raw.slice(start, end + 1)
            try {
               tempMsg = JSON.parse(jsonPart)
            } catch {
               setErrorTitle(raw)
               setErrors("")
               return
            }
         } else {
            setErrorTitle(raw)
            setErrors("")
            return
         }
      }

      if ("title" in tempMsg)
         setErrorTitle(tempMsg.title as string)
      else if ("message" in tempMsg)
         setErrorTitle(tempMsg.message as string)

      if ("errors" in tempMsg){
         const errField = tempMsg.errors
         if (Array.isArray(errField)) {
            setErrors(errField)
         } else if (errField && typeof errField === "object") {
            setErrors(Object.values(errField).flat())
         }
      }
   }

   return (
      <>
         <div className={`error-popup ${isShown ? "show" : "hide"}`} ref={popUpDisplay}>
            <div className='error-main'>

               <div className='error-content-container'>
                  <img src={CancelSVG} />
                  <div className='error-content'>
                     <div id='error-title'>Error{errorTitle ? ": " + errorTitle: ""}</div>
                     {typeof errors == "object" ? Object.entries(errors).map(([key, error]) => (
                        <div key={key} className='error-msg'>• {error}</div>
                     )) : 
                     <div className='error-msg'>{errors}</div>
                     }
                  </div>
               </div>

               <div onClick={() => handleCloseClick()} className='close-btn'>
                  <img src={CloseSVG}/>
               </div>

            </div>
         </div>
      </>
   )
}

export default ErrorPopup