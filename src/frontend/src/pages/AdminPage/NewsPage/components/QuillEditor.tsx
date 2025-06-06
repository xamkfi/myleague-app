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
      [{ 'color': [] }, { 'background': [] }],
      [{ 'align': [] }],
      ['blockquote', 'code-block'],
      ['image', 'link'],
      ['clean']
    ],
  },
};

const formats = [
  'header', 'bold', 'italic', 'underline', 'strike',
  'list', 'bullet', 'indent',
  'color', 'background', 'align',
  'blockquote', 'code-block',
  'image', 'link'
];

function QuillEditor({value, setValue}: Values){

  return (
    <div className="quill-editor-wrapper">
      <ReactQuill
        theme="snow"
        value={value}
        onChange={setValue}
        className='QuillEditor'
        modules={modules}
        formats={formats}
        placeholder="Start writing your article content here..."
      />
    </div>
  );
};

export default QuillEditor;
