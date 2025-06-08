import ReactQuill from 'react-quill';
import 'react-quill/dist/quill.snow.css';
import "../styles/QuillEditor.css"
import { handleImageUpload } from '../Services/UploadImage';
import { useMemo, useRef } from "react";

interface Values{
    value: string,
    setValue: (val: string)=>void
}

export default function QuillEditor({value, setValue}: Values) {

    const quillRef = useRef<ReactQuill | null>(null);  // Initialize the ref with null type
    
    const openImageUploader = () => {
        const input = document.createElement("input");
        input.type = "file";
        input.accept = "image/*";
      
        input.onchange = async () => {

          if (input.files?.length) {
            const file = input.files[0];
      
            try {
              const imageUrl = await handleImageUpload(file); // Call your service here
              console.log("Uploaded image URL:", imageUrl);
              
            if (quillRef.current) {
                const quill = quillRef.current.getEditor(); // Access the Quill editor instance
    
                // Insert the image into the editor at the current cursor position
                const range = quill.getSelection();
                if (range) {
                    quill.insertEmbed(range.index, "image", imageUrl); // Insert at cursor
                    quill.setSelection(range.index + 1); // Move cursor after image
                }
              }
            } catch (error) {
              alert("Image upload failed.");
            }
          }
        };
      
        input.click();
      };

    const modules = useMemo(() => ({
      toolbar: {
        container: [
          [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
          ['bold', 'italic', 'underline', "strike"],
          [{ 'list': 'ordered' }, { 'list': 'bullet' },
          { 'indent': '-1' }, { 'indent': '+1' }],
          ['image', "link",]
        ],
        handlers: {
          image: openImageUploader
        }
      },
    }), [])

  return (
    <>
        <ReactQuill
            ref={(element =>{
              if(element != null){
                quillRef.current = element
              }
            })}
            className='QuillEditor'
            theme="snow"
            value={value}
            onChange={setValue} 
            modules={modules}
        />
    </>
  )
}
