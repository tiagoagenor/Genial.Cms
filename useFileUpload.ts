import { useState, useCallback } from 'react';

interface UploadResponse {
  data: {
    fileId: string;
    fileName: string;
    filePath: string;
    fileSize: number;
    contentType: string;
    url: string;
    uploadedAt: string;
  };
}

interface UseFileUploadOptions {
  apiUrl?: string;
  getAuthToken: () => string | null;
  onProgress?: (progress: number) => void;
  onSuccess?: (data: UploadResponse['data']) => void;
  onError?: (error: string) => void;
}

interface UseFileUploadReturn {
  uploadFile: (file: File) => Promise<UploadResponse['data'] | null>;
  uploadProgress: number;
  isUploading: boolean;
  error: string | null;
  reset: () => void;
}

export const useFileUpload = (options: UseFileUploadOptions): UseFileUploadReturn => {
  const {
    apiUrl = 'http://localhost:5000/v1/files/upload',
    getAuthToken,
    onProgress,
    onSuccess,
    onError,
  } = options;

  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const updateProgress = useCallback((progress: number) => {
    setUploadProgress(progress);
    onProgress?.(progress);
  }, [onProgress]);

  const uploadFile = useCallback(async (file: File): Promise<UploadResponse['data'] | null> => {
    const token = getAuthToken();
    if (!token) {
      const errorMsg = 'Token de autenticação não encontrado';
      setError(errorMsg);
      onError?.(errorMsg);
      return null;
    }

    setIsUploading(true);
    setError(null);
    setUploadProgress(0);

    const formData = new FormData();
    formData.append('file', file);

    try {
      const xhr = new XMLHttpRequest();

      const uploadPromise = new Promise<UploadResponse>((resolve, reject) => {
        xhr.upload.addEventListener('progress', (event) => {
          if (event.lengthComputable) {
            const percentComplete = Math.round((event.loaded / event.total) * 100);
            updateProgress(percentComplete);
          }
        });

        xhr.addEventListener('load', () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            try {
              const response = JSON.parse(xhr.responseText);
              resolve(response);
            } catch (e) {
              reject(new Error('Erro ao processar resposta do servidor'));
            }
          } else {
            try {
              const errorResponse = JSON.parse(xhr.responseText);
              reject(new Error(errorResponse.title || `Erro ${xhr.status}: ${xhr.statusText}`));
            } catch (e) {
              reject(new Error(`Erro ${xhr.status}: ${xhr.statusText}`));
            }
          }
        });

        xhr.addEventListener('error', () => {
          reject(new Error('Erro de rede ao fazer upload'));
        });

        xhr.addEventListener('abort', () => {
          reject(new Error('Upload cancelado'));
        });
      });

      xhr.open('POST', apiUrl);
      xhr.setRequestHeader('Authorization', `Bearer ${token}`);
      xhr.send(formData);

      const response = await uploadPromise;
      const result = response.data;
      
      onSuccess?.(result);
      setIsUploading(false);
      setUploadProgress(0);
      
      return result;
    } catch (err) {
      const errorMsg = err instanceof Error ? err.message : 'Erro desconhecido ao fazer upload';
      setError(errorMsg);
      onError?.(errorMsg);
      setIsUploading(false);
      setUploadProgress(0);
      return null;
    }
  }, [apiUrl, getAuthToken, updateProgress, onSuccess, onError]);

  const reset = useCallback(() => {
    setUploadProgress(0);
    setIsUploading(false);
    setError(null);
  }, []);

  return {
    uploadFile,
    uploadProgress,
    isUploading,
    error,
    reset,
  };
};
