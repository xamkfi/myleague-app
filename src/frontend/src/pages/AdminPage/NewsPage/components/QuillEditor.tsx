import ReactQuill from 'react-quill';
import 'react-quill/dist/quill.snow.css';
import "../NewsCreatePage.scss";
import { handleImageUploadService } from '../../../../api/admin/News/handleImageUploadService';
import { useEffect, useMemo, useRef } from "react";
import { handleImageDeleteService } from '../../../../api/admin/News/handleImageDeleteService';

interface Values{
    value: string,
    setValue: (val: string)=>void,
    setLoading: (val: boolean)=>void
}

export default function QuillEditor({value, setValue, setLoading}: Values) {

    const quillRef = useRef<ReactQuill | null>(null);  // Initialize the ref with null type
    const previousImagesRef = useRef<string[]>([]); //Save previous images

    const extractImageUrls = (html: string): string[] => {
      const div = document.createElement("div");
      div.innerHTML = html;
      const imgTags = div.querySelectorAll("img");
      return Array.from(imgTags).map((img)=> img.getAttribute("src") || "").filter(Boolean);
    }

  useEffect(() => {
    const currentImages = extractImageUrls(value);
    const previousImages = previousImagesRef.current;

    const deletedImages = previousImages.filter((url) => !currentImages.includes(url));


    if (deletedImages.length > 0) {
      const confirmDelete = window.confirm(
        `Haluatko varmasti poistaa ${deletedImages.length} kuva${deletedImages.length > 1 ? 'a' : 'n'}?`
      );

      if (confirmDelete) {
        deletedImages.forEach((url) => {
          handleImageDeleteService(url).catch((err) => {
            console.error("Failed to delete image:", err);
          });
        });
      } else {

        if (quillRef.current) {
          const quill = quillRef.current.getEditor();
          deletedImages.forEach(url => {
            const range = quill.getSelection();
            const index = range ? range.index : quill.getLength();
            quill.insertEmbed(index, "image", url);
          });
        }
      }
    }

    previousImagesRef.current = currentImages;
  }, [value]);

    const openImageUploader = () => {
        const input = document.createElement("input");
        input.type = "file";
        input.accept = "image/*";
      
        input.onchange = async () => {

          if (input.files?.length) {
            const file = input.files[0];
      
            try {
              setLoading(true);
              const imageUrl = await handleImageUploadService(file); // Call your service here
              console.log("Uploaded image URL:", imageUrl);
              
            if (quillRef.current) {
                const quill = quillRef.current.getEditor(); // Access the Quill editor instance
    
                // Insert the image into the editor at the current cursor position
                const range = quill.getSelection();
                if (range) {
                    quill.insertEmbed(range.index, "image", imageUrl); // Insert at cursor
                    quill.setSelection({ index: range.index + 1, length: 0 });// Move cursor after image
                }
              }
              setLoading(false);
            } catch (error) {
              console.error("Image upload error:", error);
              setLoading(false);
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
    }), [openImageUploader])

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
