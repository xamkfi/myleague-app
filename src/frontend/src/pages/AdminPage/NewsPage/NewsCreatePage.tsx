import { useState } from "react";
import PageTemplate from "../../../components/PageTemplate/PageTemplate";
import QuillEditor from "./components/QuillEditor";
import { useTranslation } from "react-i18next";
import PreviewNews from "./components/PreviewNews";
import NewsInputs from "./components/NewsInputs";
import { type NewsInputsData } from "./components/NewsInputs";

export default function NewsCreatePage() {

    const { t } = useTranslation();
    const [value, setValue] = useState("");
    const [preview, setPreview] = useState(true);

    const [newsData, setNewsData] = useState<NewsInputsData>({
        title: '',
        mainPicture: '',
        author: '',
        tags: [],
        category: '',
        sportCategory: ''
    });
  return (
    <>
    <PageTemplate title={t('admin.title', 'Admin Dashboard')}>

        <div>
            <h1>Hello world!!</h1>
            <button onClick={()=>setPreview(!preview)} className="bg-green-400">Preview</button>
        </div>

        {preview ? 
        <div className="">
            <NewsInputs     
            data={newsData}
            onChange={setNewsData}
            />
            
            <QuillEditor value={value} setValue={setValue}/>
        </div>
            :
        <div className="">
            <PreviewNews value={value}/> 
        </div> 
        }

    </PageTemplate>
    </>
  )
}