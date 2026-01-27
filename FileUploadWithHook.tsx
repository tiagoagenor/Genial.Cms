import React, { useState, useRef } from 'react';
import { useFileUpload } from './useFileUpload';

const FileUploadWithHook: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadResult, setUploadResult] = useState<any>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const getAuthToken = (): string | null => {
    return localStorage.getItem('authToken');
  };

  const { uploadFile, uploadProgress, isUploading, error, reset } = useFileUpload({
    getAuthToken,
    onSuccess: (data) => {
      setUploadResult(data);
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    },
    onError: (errorMsg) => {
      console.error('Erro no upload:', errorMsg);
    },
  });

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      reset();
      setUploadResult(null);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) return;
    await uploadFile(selectedFile);
  };

  const handleCancel = () => {
    setSelectedFile(null);
    reset();
    setUploadResult(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
  };

  return (
    <div style={{ padding: '20px', maxWidth: '600px', margin: '0 auto' }}>
      <h2>Upload de Arquivo (com Hook)</h2>

      <div style={{ marginBottom: '20px' }}>
        <input
          ref={fileInputRef}
          type="file"
          onChange={handleFileChange}
          disabled={isUploading}
          style={{ marginBottom: '10px' }}
        />
        {selectedFile && (
          <div style={{ marginTop: '10px', padding: '10px', backgroundColor: '#f5f5f5', borderRadius: '4px' }}>
            <p><strong>Arquivo:</strong> {selectedFile.name}</p>
            <p><strong>Tamanho:</strong> {formatFileSize(selectedFile.size)}</p>
          </div>
        )}
      </div>

      {isUploading && (
        <div style={{ marginBottom: '20px' }}>
          <div style={{ 
            width: '100%', 
            backgroundColor: '#e0e0e0', 
            borderRadius: '4px', 
            overflow: 'hidden',
            marginBottom: '10px'
          }}>
            <div
              style={{
                width: `${uploadProgress}%`,
                backgroundColor: '#4caf50',
                height: '30px',
                transition: 'width 0.3s ease',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'white',
                fontWeight: 'bold'
              }}
            >
              {uploadProgress}%
            </div>
          </div>
        </div>
      )}

      {error && (
        <div style={{ 
          padding: '10px', 
          backgroundColor: '#ffebee', 
          color: '#c62828', 
          borderRadius: '4px',
          marginBottom: '20px'
        }}>
          <strong>Erro:</strong> {error}
        </div>
      )}

      {uploadResult && (
        <div style={{ 
          padding: '15px', 
          backgroundColor: '#e8f5e9', 
          borderRadius: '4px',
          marginBottom: '20px'
        }}>
          <h3 style={{ marginTop: 0, color: '#2e7d32' }}>Upload realizado com sucesso!</h3>
          <p><strong>Nome:</strong> {uploadResult.fileName}</p>
          <p><strong>Tamanho:</strong> {formatFileSize(uploadResult.fileSize)}</p>
          <p><strong>URL:</strong> 
            <a 
              href={`http://localhost:5000${uploadResult.url}`} 
              target="_blank" 
              rel="noopener noreferrer"
              style={{ marginLeft: '5px', color: '#1976d2' }}
            >
              {uploadResult.url}
            </a>
          </p>
        </div>
      )}

      <div style={{ display: 'flex', gap: '10px' }}>
        <button
          onClick={handleUpload}
          disabled={!selectedFile || isUploading}
          style={{
            padding: '10px 20px',
            backgroundColor: isUploading || !selectedFile ? '#ccc' : '#2196f3',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
            cursor: isUploading || !selectedFile ? 'not-allowed' : 'pointer',
            fontSize: '16px'
          }}
        >
          {isUploading ? 'Enviando...' : 'Fazer Upload'}
        </button>

        {(selectedFile || isUploading) && (
          <button
            onClick={handleCancel}
            disabled={isUploading}
            style={{
              padding: '10px 20px',
              backgroundColor: '#f44336',
              color: 'white',
              border: 'none',
              borderRadius: '4px',
              cursor: isUploading ? 'not-allowed' : 'pointer',
              fontSize: '16px'
            }}
          >
            Cancelar
          </button>
        )}
      </div>
    </div>
  );
};

export default FileUploadWithHook;
