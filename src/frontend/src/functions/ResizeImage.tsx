
export const ResizeImage = (file: File, targetWidth: number, targetHeight: number): Promise<File> => {
    return new Promise((resolve) => {
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d')!;
      const img = new Image();
      
      img.onload = () => {
        // Set canvas dimensions to target size
        canvas.width = targetWidth;
        canvas.height = targetHeight;
        
        // Calculate scaling to maintain aspect ratio
        const scale = Math.min(targetWidth / img.width, targetHeight / img.height);
        const scaledWidth = img.width * scale;
        const scaledHeight = img.height * scale;
        
        // Center the image
        const x = (targetWidth - scaledWidth) / 2;
        const y = (targetHeight - scaledHeight) / 2;
        
        // Draw the resized image
        ctx.drawImage(img, x, y, scaledWidth, scaledHeight);
        
        // Convert to blob
        canvas.toBlob((blob) => {
          const resizedFile = new File([blob!], file.name, {
            type: 'image/jpeg',
            lastModified: Date.now(),
          });
          resolve(resizedFile);
        }, 'image/jpeg', 0.85); // 85% quality
      };
      
      img.src = URL.createObjectURL(file);
    });
  };