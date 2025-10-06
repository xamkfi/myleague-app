import { useEffect, useState } from 'react'
import './ErrorPopup.scss'
import CloseSVG from '../../assets/basicIcons/close.svg'

interface ErrorPopupProps {
   message?: string | null
   onClose?: () => void
}

function ErrorPopup({message, onClose}: ErrorPopupProps) {
   const [visible, setVisible] = useState(false)

   useEffect(() => {
      if (message) {
         const id = requestAnimationFrame(() => setVisible(true))
         return () => cancelAnimationFrame(id)
      }
   }, [message])

   if (!message) return null

   let errorContent = <>{message}</>;

   try {
      const data = JSON.parse(message);
      let titleMessage = '';
      let errorDetails = [];

      if (data.message && data.errors && typeof data.errors === 'object' && !Array.isArray(data.errors)) {
         titleMessage = data.message;
         errorDetails = Object.entries(data.errors).map(([field, msgs]) => (
            <div key={field}>
               <strong>{field}:</strong>
               <ul>
                  {(Array.isArray(msgs) ? msgs : [msgs]).map((msg, idx) => <li key={idx}>{msg}</li>)}
               </ul>
            </div>
         ));
      } else if (data.message && data.errors && Array.isArray(data.errors)) {
         titleMessage = data.message;
         errorDetails = data.errors.map((msg: string, idx: number) => <li key={idx}>{msg}</li>);
      }

      if (titleMessage) {
         errorContent = (
            <>
               <h3>{titleMessage}</h3>
               <ul>{errorDetails}</ul>
            </>
         );
      }
   } catch {
      errorContent = <>{message}</>;
   }


   const handleClose = () => {
      setVisible(false)
   }

   const handleTransitionEnd = () => {
      if (!visible) {
         onClose?.()
      }
   }

   return (
      <>
         <div className={`error-popup ${visible ? 'show' : 'hide'}`} onTransitionEnd={handleTransitionEnd} role='alert' aria-live='assertive'>
            <img src={CloseSVG} alt='' aria-hidden='true' />
            <div id='error-msg'>
               {errorContent}
            </div>
            <div className='close-btn' onClick={handleClose} aria-label='Close'>x</div>
         </div>
      </>
   )
}

export default ErrorPopup