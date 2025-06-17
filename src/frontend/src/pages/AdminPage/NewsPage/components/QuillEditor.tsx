import ReactQuill, {Quill} from 'react-quill';
import 'react-quill/dist/quill.snow.css';
import "../styles/QuillEditor.scss";
import { handleImageUploadService } from '../../../../api/admin/News/handleImageUploadService';
import { useEffect, useMemo, useRef } from "react";
import { handleImageDeleteService } from '../../../../api/admin/News/handleImageDeleteService';
import MatchSelectionHeader from './MatchSelectionHeader';
import type { FloorballMatch } from '../../../../api/admin/News/GetMatchesService';
import "../styles/MatchResult.scss";


interface Values{
    value: string,
    setValue: (val: string)=>void,
    setLoading: (val: boolean)=>void
}

export interface MatchResultValue {
  homeTeam: string;
  awayTeam: string;
  homeScore: string;
  awayScore: string;
  date: string;
  link: string;
}

const BlockEmbed = Quill.import('blots/block/embed') as any;

export class MatchResultTableBlot extends BlockEmbed {
  static blotName = 'matchResultTable';
  static tagName = 'div';
  static className = 'match-result-table-container';

  static create(value: { matches: any[], title?: string }): HTMLElement {
    const node = super.create();
    const { matches, title } = value;

    const tableRows = matches.map(match => `
      <tr class="match-result-table__row">
        <td class="match-result-table__date">${match.date}</td>
        <td class="match-result-table__teams">
          ${match.homeTeam} ${match.homeScore} - ${match.awayScore} ${match.awayTeam}
        </td>
        <td class="match-result-table__status">
          <span class="status-badge status-${match.status}">
            ${match.status}
          </span>
        </td>
        <td class="match-result-table__link">
          <a href="${match.link}" target="_blank" rel="noopener noreferrer">View Details</a>
        </td>
      </tr>
    `).join('');

    node.innerHTML = `
      <div class="match-result-table">
        ${title ? `<h4 class="match-result-table__title">${title}</h4>` : ''}
        <table class="match-result-table__table">
          <thead class="match-result-table__header">
            <tr>
              <th>Päivä</th>
              <th>Ottelu</th>
              <th>Tila</th>
              <th>Toiminnot</th>
            </tr>
          </thead>
          <tbody class="match-result-table__body">
            ${tableRows}
          </tbody>
        </table>
        <!-- Piilotettu data JSON-muodossa -->
        <script type="application/json" class="match-result-data" style="display: none;">
          ${JSON.stringify({ matches, title })}
        </script>
      </div>
    `;

    node.setAttribute('contenteditable', 'false');
    return node;
  }

  static value(node: HTMLElement) {
    const dataElement = node.querySelector('.match-result-data');
    if (dataElement && dataElement.textContent) {
      try {
        return JSON.parse(dataElement.textContent);
      } catch (e) {
        console.error('Error parsing match result data:', e);
      }
    }
    
    return { matches: [], title: '' };
  }
}

Quill.register(MatchResultTableBlot);

export default function QuillEditor({value, setValue, setLoading}: Values) {
    const quillRef = useRef<ReactQuill | null>(null);
    const previousImagesRef = useRef<string[]>([]);
    const previousMatchResultsRef = useRef<any[]>([]);

    const extractImageUrls = (html: string): string[] => {
      const div = document.createElement("div");
      div.innerHTML = html;
      const imgTags = div.querySelectorAll("img");
      return Array.from(imgTags).map((img)=> img.getAttribute("src") || "").filter(Boolean);
    }

    const extractMatchResults = (html: string): any[] => {
      const div = document.createElement("div");
      div.innerHTML = html;
      const matchResultElements = div.querySelectorAll('.match-result-table-container');
      
      return Array.from(matchResultElements).map(element => {
        const dataElement = element.querySelector('.match-result-data');
        if (dataElement && dataElement.textContent) {
          try {
            return JSON.parse(dataElement.textContent);
          } catch (e) {
            console.error('Error parsing match result data:', e);
          }
        }
        return null;
      }).filter(Boolean);
    }

    const handleElementDeletion = (deletedImages: string[], deletedMatchResults: any[]) => {
      const totalElements = deletedImages.length + deletedMatchResults.length;
      
      if (totalElements === 0) return;

      let message = 'Haluatko varmasti poistaa ';
      
      if (deletedImages.length > 0 && deletedMatchResults.length > 0) {
        message += `${deletedImages.length} kuva${deletedImages.length > 1 ? 'a' : 'n'} ja ${deletedMatchResults.length} ottelutulosta?`;
      } else if (deletedImages.length > 0) {
        message += `${deletedImages.length} kuva${deletedImages.length > 1 ? 'a' : 'n'}?`;
      } else {
        message += `${deletedMatchResults.length} ottelutulosta?`;
      }

      const confirmDelete = window.confirm(message);

      if (confirmDelete) {
        // Poista kuvat palvelimelta
        deletedImages.forEach((url) => {
          handleImageDeleteService(url).catch((err) => {
            console.error("Failed to delete image:", err);
          });
        });
      } else {
        // Palauta poistetut elementit editoriin
        if (quillRef.current) {
          const quill = quillRef.current.getEditor();
          const range = quill.getSelection();
          const index = range ? range.index : quill.getLength();

          // Palauta kuvat
          deletedImages.forEach(url => {
            quill.insertEmbed(index, "image", url);
          });

          // Palauta ottelutulokset
          deletedMatchResults.forEach(matchResult => {
            quill.insertEmbed(index, 'matchResultTable', matchResult);
          });
        }
      }
    };

    useEffect(() => {
      const currentImages = extractImageUrls(value);
      const currentMatchResults = extractMatchResults(value);
      const previousImages = previousImagesRef.current;
      const previousMatchResults = previousMatchResultsRef.current;

      const deletedImages = previousImages.filter((url) => !currentImages.includes(url));
      const deletedMatchResults = previousMatchResults.filter((prevResult) => 
        !currentMatchResults.some(currentResult => 
          JSON.stringify(currentResult) === JSON.stringify(prevResult)
        )
      );

      handleElementDeletion(deletedImages, deletedMatchResults);

      previousImagesRef.current = currentImages;
      previousMatchResultsRef.current = currentMatchResults;
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
              const imageUrl = await handleImageUploadService(file);
              console.log("Uploaded image URL:", imageUrl);
              
              if (quillRef.current) {
                const quill = quillRef.current.getEditor();
                const range = quill.getSelection();
                if (range) {
                    quill.insertEmbed(range.index, "image", imageUrl);
                    quill.setSelection({ index: range.index + 1, length: 0 });
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
    }), [])

    const handleInsertMatches = (matches: FloorballMatch[]) => {
        const editor = quillRef.current?.getEditor();
        const range = editor?.getSelection(true);
        
        if (editor && range && matches.length > 0) {
            const matchesData = matches.map(match => ({
                homeTeam: match.homeTeamName,
                awayTeam: match.awayTeamName,
                homeScore: match.homeScore,
                awayScore: match.awayScore,
                date: match.scheduledDateTime,
                status: match.status,
                link: match.id
            }));

            editor.insertEmbed(range.index, 'matchResultTable', {
                matches: matchesData,
                title: "Valitut ottelut"
            });
            editor.setSelection({ index: range.index + 1, length: 0 });
        }
    };

    return (
        <>
            <MatchSelectionHeader onInsertMatches={handleInsertMatches} />
            
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
