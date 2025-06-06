import ReactQuill from 'react-quill';
import 'react-quill/dist/quill.snow.css'; // Import the Quill theme
import "../NewsCreatePage.scss"

interface Values{
    value: string,
    setValue: (val: string)=>void
}

const modules = {
  toolbar: {
    container: [
      [{ header: [1, 2, 3, 4, 5, 6, false] }],
      ['bold', 'italic', 'underline', 'strike'],
      [{ list: 'ordered' }, { list: 'bullet' }, { indent: '-1' }, { indent: '+1' }],
      ['image', 'link']
    ],
  },
};

function QuillEditor({value, setValue}: Values){

  return (
    
      <ReactQuill
        theme="snow"
        value={value}
        onChange={setValue}
        className='QuillEditor'
        modules={modules}
      />

  );
};

export default QuillEditor;
