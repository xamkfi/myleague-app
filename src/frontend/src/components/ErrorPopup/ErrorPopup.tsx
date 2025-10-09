import { useEffect, useState, useRef } from 'react'
import './ErrorPopup.scss'
import CloseSVG from '../../assets/basicIcons/close.svg'

interface ErrorPopupProps {
   message: string | null,
}

function ErrorPopup({message}: ErrorPopupProps) {
   const [isShown, setIsShown] = useState<boolean>(false)
   const popUpDisplay = useRef<HTMLDivElement>(null)

   const handleCloseClick = () => {
      setIsShown(false)
   }

   useEffect(() => {
      if (message && message?.length > 1){
         setIsShown(true)
         console.log(message)
      }
   }, [message])


   return (
      <>
         <div className={`error-popup ${isShown ? "show" : "hide"}`} ref={popUpDisplay}>
            <img src={CloseSVG} />
            <div id='error-msg'>{message}</div>
            <div onClick={() => handleCloseClick()} className='close-btn'>x</div>
         </div>
      </>
   )
}

export default ErrorPopup