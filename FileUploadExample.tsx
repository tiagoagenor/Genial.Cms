import React, { useState, useRef } from 'react';

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

const FileUploadComponent: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadResult, setUploadResult] = useState<UploadResponse['data'] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Obter token do localStorage ou contexto de autenticação
  const getAuthToken = (): string | null => {
    // Substitua isso pela sua lógica de obtenção do token
    return localStorage.getItem('authToken');
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (file) {
      setSelectedFile(file);
      setError(null);
      setUploadResult(null);
      setUploadProgress(0);
    }
  };

  const handleUpload = async () => {
    if (!selectedFile) {
      setError('Por favor, selecione um arquivo');
      return;
    }

    const token = getAuthToken();
    if (!token) {
      setError('Token de autenticação não encontrado');
      return;
    }

    setIsUploading(true);
    setError(null);
    setUploadProgress(0);

    // Criar FormData
    const formData = new FormData();
    formData.append('file', selectedFile);

    try {
      // Usar XMLHttpRequest para ter controle sobre o progresso
      const xhr = new XMLHttpRequest();

      // Configurar evento de progresso
      xhr.upload.addEventListener('progress', (event) => {
        if (event.lengthComputable) {
          const percentComplete = Math.round((event.loaded / event.total) * 100);
          setUploadProgress(percentComplete);
        }
      });

      // Criar Promise para lidar com a resposta
      const uploadPromise = new Promise<UploadResponse>((resolve, reject) => {
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

      // Configurar e enviar requisição
      xhr.open('POST', 'http://localhost:5000/v1/files/upload');
      xhr.setRequestHeader('Authorization', `Bearer ${token}`);
      
      // Não definir Content-Type manualmente - o browser define automaticamente com boundary para multipart/form-data
      
      xhr.send(formData);

      // Aguardar resposta
      const response = await uploadPromise;
      setUploadResult(response.data);
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Erro desconhecido ao fazer upload');
    } finally {
      setIsUploading(false);
      setUploadProgress(0);
    }
  };

  const handleCancel = () => {
    setSelectedFile(null);
    setUploadProgress(0);
    setError(null);
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
      <h2>Upload de Arquivo</h2>

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
            <p><strong>Arquivo selecionado:</strong> {selectedFile.name}</p>
            <p><strong>Tamanho:</strong> {formatFileSize(selectedFile.size)}</p>
            <p><strong>Tipo:</strong> {selectedFile.type || 'Não especificado'}</p>
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
          <p style={{ textAlign: 'center', color: '#666' }}>
            Enviando arquivo... {uploadProgress}%
          </p>
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
          <p><strong>ID do arquivo:</strong> {uploadResult.fileId}</p>
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
          <p><strong>Enviado em:</strong> {new Date(uploadResult.uploadedAt).toLocaleString()}</p>
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

export default FileUploadComponent;
