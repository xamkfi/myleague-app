import "../NewsCreatePage.scss"

interface Value{
    value: string
}

export default function PreviewNews({value}: Value){

    return(<>
        <div className="PreviewNews" dangerouslySetInnerHTML={{__html: value}}/>
    </>)
}