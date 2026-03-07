import ReactQuill, {Quill} from 'react-quill';
import 'react-quill/dist/quill.snow.css';
import "../styles/QuillEditor.scss";
import { handleImageUploadService } from '../../../../api/admin/News/handleImageUploadService';
import { useEffect, useMemo, useRef, useCallback } from "react";
import { handleImageDeleteService } from '../../../../api/admin/News/handleImageDeleteService';
import MatchSelectionHeader from './MatchSelectionHeader';
import type { FloorballMatch } from '../../../../api/admin/News/GetMatchesService';
import "../styles/MatchResult.scss";


interface Values{
    value: string,
    setValue: (val: string)=>void,
    setLoading: (val: boolean)=>void,
    isClearing?: boolean
}

export interface MatchResultValue {
  homeTeam: string;
  awayTeam: string;
  homeScore: string;
  awayScore: string;
  date: string;
  link: string;
  status?: string;
  homeTeamImage?: string;
  awayTeamImage?: string;
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const BlockEmbed = Quill.import('blots/block/embed') as any;

export class MatchResultTableBlot extends BlockEmbed {
  static blotName = 'matchResultTable';
  static tagName = 'div';
  static className = 'match-result-table-container';

  static create(value: { matches: MatchResultValue[], title?: string }): HTMLElement {
    const node = super.create();
    const { matches } = value;

    const matchRows = matches.map(match => {
      const homeTeamImg = match.homeTeamImage ? `<img src="${match.homeTeamImage}" alt="${match.homeTeam}" class="mr-team-logo" />` : '<span class="mr-team-logo-placeholder"></span>';
      const awayTeamImg = match.awayTeamImage ? `<img src="${match.awayTeamImage}" alt="${match.awayTeam}" class="mr-team-logo" />` : '<span class="mr-team-logo-placeholder"></span>';

      const isCompleted = match.status && match.status.toLowerCase() === 'completed';
      const homeScore = isCompleted ? match.homeScore : '-';
      const awayScore = isCompleted ? match.awayScore : '-';
      const homeWon = isCompleted && Number(match.homeScore) > Number(match.awayScore);
      const awayWon = isCompleted && Number(match.awayScore) > Number(match.homeScore);

      const dateStr = new Date(match.date).toLocaleDateString("fi-FI", { day: 'numeric', month: 'numeric' });
      const timeStr = new Date(match.date).toLocaleTimeString("fi-FI", { hour: '2-digit', minute: '2-digit' });

      const statusClass = match.status ? `mr-status-dot--${match.status.toLowerCase()}` : '';

      return `<a href="/match/${match.link}" class="match-result-row" target="_blank" rel="noopener noreferrer"><span class="mr-date"><span class="mr-date-day">${dateStr}</span><span class="mr-date-time">${timeStr}</span></span><span class="mr-teams"><span class="mr-team-line${homeWon ? ' mr-winner' : ''}">${homeTeamImg}<span class="mr-team-name">${match.homeTeam}</span></span><span class="mr-team-line${awayWon ? ' mr-winner' : ''}">${awayTeamImg}<span class="mr-team-name">${match.awayTeam}</span></span></span><span class="mr-scores"><span class="mr-score${homeWon ? ' mr-score--winner' : ''}">${homeScore}</span><span class="mr-score${awayWon ? ' mr-score--winner' : ''}">${awayScore}</span></span><span class="mr-status"><span class="mr-status-dot ${statusClass}"></span></span></a>`;
    }).join('');
    node.innerHTML = `<div class="match-result-list">${matchRows}</div><script type="application/json" class="match-result-data" style="display: none;">${JSON.stringify({ matches })}</script>`;
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

export default function QuillEditor({value, setValue, setLoading, isClearing = false}: Values) {
    const quillRef = useRef<ReactQuill | null>(null);
    const previousImagesRef = useRef<string[]>([]);
    const previousMatchResultsRef = useRef<MatchResultValue[]>([]);
    const isNavigatingRef = useRef(false);
    
    const extractImageUrls = (html: string): string[] => {
      const div = document.createElement("div");
      div.innerHTML = html;
      const imgTags = div.querySelectorAll("img");
      return Array.from(imgTags).map((img)=> img.getAttribute("src") || "").filter(Boolean);
    }

    const extractMatchResults = (html: string): MatchResultValue[] => {
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
      }).filter(Boolean) as MatchResultValue[];
    }

    const handleElementDeletion = useCallback((deletedImages: string[], deletedMatchResults: MatchResultValue[]) => {
      const totalElements = deletedImages.length + deletedMatchResults.length;
      
      if (totalElements === 0) return;

      if (isClearing) {
        deletedImages.forEach((url) => {
          handleImageDeleteService(url).catch((err) => {
            console.error("Failed to delete image:", err);
          });
        });
        return;
      }

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
        deletedImages.forEach((url) => {
          handleImageDeleteService(url).catch((err) => {
            console.error("Failed to delete image:", err);
          });
        });
      } else {
        if (quillRef.current) {
          const quill = quillRef.current.getEditor();
          const range = quill.getSelection();
          const index = range ? range.index : quill.getLength();

          deletedImages.forEach(url => {
            quill.insertEmbed(index, "image", url);
          });

          deletedMatchResults.forEach(matchResult => {
            quill.insertEmbed(index, 'matchResultTable', matchResult);
          });
        }
      }
    }, [isClearing]);

    useEffect(() => {
      if (isNavigatingRef.current) {
        console.log("🚫 Navigation in progress - skipping useEffect completely");
        return;
      }
      
      const currentImages = extractImageUrls(value);
      const currentMatchResults = extractMatchResults(value);
      const previousImages = previousImagesRef.current;
      const previousMatchResults = previousMatchResultsRef.current;

      if (previousImages.length === 0 && previousMatchResults.length === 0) {
        console.log("First render - setting initial state");
        previousImagesRef.current = currentImages;
        previousMatchResultsRef.current = currentMatchResults;
        return;
      }

      const deletedImages = previousImages.filter((url) => !currentImages.includes(url));
      const deletedMatchResults = previousMatchResults.filter((prevResult) => 
        !currentMatchResults.some(currentResult => 
          JSON.stringify(currentResult) === JSON.stringify(prevResult)
        )
      );

      console.log("Deleted images:", deletedImages);

      if (deletedImages.length > 0 || deletedMatchResults.length > 0) {
        console.log('Elements deleted, calling handleElementDeletion');
        handleElementDeletion(deletedImages, deletedMatchResults);
      }

      previousImagesRef.current = currentImages;
      previousMatchResultsRef.current = currentMatchResults;
    }, [value, handleElementDeletion]);

    useEffect(() => {
      if (typeof window !== 'undefined') {
        window.setQuillNavigatingState = (isNavigating: boolean) => {
          isNavigatingRef.current = isNavigating;
          console.log("QuillEditor navigation state set to:", isNavigating);
        };
      }
    }, []);

    const openImageUploader = useCallback(() => {
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
    }, [setLoading]);

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
                link: match.id,
                homeTeamImage: match.homeTeamLogo ?? undefined,
                awayTeamImage: match.awayTeamLogo ?? undefined
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
