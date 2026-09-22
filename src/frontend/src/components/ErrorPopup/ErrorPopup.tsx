import { useEffect, useState, useRef } from 'react'
import { useTranslation } from 'react-i18next'
import './ErrorPopup.scss'
import CancelSVG from '../../assets/basicIcons/cancel.svg'
import CloseSVG from '../../assets/basicIcons/close.svg'
import type { ParsedError } from '../../types/common/errorTypes'

interface ErrorPopupProps {
   message: string | null,
}

interface ParsedPopupContent {
   title: string
   errors: string[]
}

function parseErrorPayload(message: string): ParsedPopupContent {
   const raw = message.replace(/^Error:\s*/, '').trim()

   const tryParse = (text: string): ParsedError | null => {
      try {
         const value: unknown = JSON.parse(text)
         if (value && typeof value === 'object' && !Array.isArray(value)) {
            return value as ParsedError
         }
         return null
      } catch {
         return null
      }
   }

   let parsed = tryParse(raw)
   if (!parsed) {
      const start = raw.indexOf('{')
      const end = raw.lastIndexOf('}')
      if (start !== -1 && end > start) {
         parsed = tryParse(raw.slice(start, end + 1))
      }
   }

   if (!parsed) {
      return { title: raw, errors: [] }
   }

   const title =
      (typeof parsed.title === 'string' && parsed.title.trim())
      || (typeof parsed.message === 'string' && parsed.message.trim())
      || ''

   let errors: string[] = []
   if (Array.isArray(parsed.errors)) {
      errors = parsed.errors.filter((item): item is string => typeof item === 'string' && item.trim().length > 0)
   } else if (parsed.errors && typeof parsed.errors === 'object') {
      errors = Object.values(parsed.errors)
         .flat()
         .filter((item): item is string => typeof item === 'string' && item.trim().length > 0)
   }

   return { title, errors }
}

function ErrorPopup({message}: ErrorPopupProps) {
   const { t } = useTranslation()
   const [errors, setErrors] = useState<string[]>([])
   const [errorTitle, setErrorTitle] = useState<string>('')
   const [isShown, setIsShown] = useState<boolean>(false)
   const popUpDisplay = useRef<HTMLDivElement>(null)

   const handleCloseClick = () => {
      setIsShown(false)
   }

   useEffect(() => {
      if (typeof message !== 'string' || message.trim().length === 0) {
         setIsShown(false)
         setErrorTitle('')
         setErrors([])
         return
      }

      const parsed = parseErrorPayload(message)
      if (!parsed.title && parsed.errors.length === 0) {
         setIsShown(false)
         setErrorTitle('')
         setErrors([])
         return
      }

      setErrorTitle(parsed.title || t('common.error'))
      setErrors(parsed.errors)
      setIsShown(true)
   }, [message, t])

   return (
      <>
         <div className={`error-popup ${isShown ? "show" : "hide"}`} ref={popUpDisplay}>
            <div className='error-main'>

               <div className='error-content-container'>
                  <img src={CancelSVG} />
                  <div className='error-content'>
                     <div id='error-title'>{errorTitle}</div>
                     {errors.map((error, index) => (
                        <div key={`${index}-${error}`} className='error-msg'>• {error}</div>
                     ))}
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
