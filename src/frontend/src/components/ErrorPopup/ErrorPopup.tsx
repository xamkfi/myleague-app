import { useEffect, useState, useRef } from 'react'
import './ErrorPopup.scss'
import CloseSVG from '../../assets/basicIcons/close.svg'

interface ErrorPopupProps {
   message: string | null,
}

function ErrorPopup({message}: ErrorPopupProps) {
   const [isShown, setIsShown] = useState<Boolean>(true)

   const handleCloseClick = () => {
      setIsShown(false)
   }

   useEffect(() => {
      setIsShown(true)
   }, [message])


   return (
      <>
         {(message && isShown) &&
            <div className={`error-popup ${isShown ? "show" : "hide"}`}>
               <img src={CloseSVG} />
               <div id='error-msg'>{message}</div>
               <div onClick={() => handleCloseClick()} className='close-btn'>x</div>
            </div>
         }
      </>
   )
}

export default ErrorPopup