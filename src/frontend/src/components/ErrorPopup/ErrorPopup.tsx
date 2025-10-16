import { useEffect, useState, useRef } from 'react'
import './ErrorPopup.scss'
import CancelSVG from '../../assets/basicIcons/cancel.svg'
import CloseSVG from '../../assets/basicIcons/close.svg'

interface ErrorPopupProps {
   message: string | null,
}

function ErrorPopup({message}: ErrorPopupProps) {
   const [errors, setErrors] = useState<Object | string>("")
   const [errorTitle, setErrorTitle] = useState<string>("")
   const [isShown, setIsShown] = useState<boolean>(false)
   const popUpDisplay = useRef<HTMLDivElement>(null)

   const handleCloseClick = () => {
      setIsShown(false)
   }

   useEffect(() => {
      if (message && message?.length > 1){
         parseErrorMessage(message)
         setIsShown(true)
      }
   }, [message])

   useEffect(() => {
      if (errors){
         console.log(errors)
      }
   }, [errors])

   const parseErrorMessage = (msg: string | null) => {
      if (typeof msg == "string"){
         if (!msg.startsWith("{")){
            setErrors(msg)
            return
         }
         const tempMsg = JSON.parse(msg)
         if ("title" in tempMsg){
            setErrorTitle(tempMsg.title)
         }else if ("message" in tempMsg){
            setErrorTitle(tempMsg.message)
         }
         console.log(tempMsg.errors)
         setErrors(tempMsg.errors)
      }
      // return msg?.split(" ").slice(1).join(" ")
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
                        <div className='error-msg'>• {key}: {error}</div>
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