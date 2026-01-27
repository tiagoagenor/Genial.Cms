/**
 * Função para fazer upload de arquivo com progresso
 * @param {File} file - Arquivo a ser enviado
 * @param {string} apiUrl - URL da API (padrão: http://localhost:5000/v1/files/upload)
 * @param {string} authToken - Token JWT de autenticação
 * @param {Function} onProgress - Callback chamado com o progresso (0-100)
 * @param {Function} onSuccess - Callback chamado em caso de sucesso
 * @param {Function} onError - Callback chamado em caso de erro
 * @returns {Promise} Promise que resolve com a resposta do servidor
 */
function uploadFile(file, apiUrl = 'http://localhost:5000/v1/files/upload', authToken, onProgress, onSuccess, onError) {
    return new Promise((resolve, reject) => {
        // Validar arquivo
        if (!file || !file.size) {
            const error = new Error('Arquivo inválido ou vazio');
            if (onError) onError(error);
            reject(error);
            return;
        }

        // Validar token
        if (!authToken) {
            const error = new Error('Token de autenticação não fornecido');
            if (onError) onError(error);
            reject(error);
            return;
        }

        // Criar FormData
        const formData = new FormData();
        formData.append('file', file);

        // Criar XMLHttpRequest para ter controle sobre o progresso
        const xhr = new XMLHttpRequest();

        // Configurar evento de progresso
        xhr.upload.addEventListener('progress', (event) => {
            if (event.lengthComputable && onProgress) {
                const percentComplete = Math.round((event.loaded / event.total) * 100);
                onProgress(percentComplete);
            }
        });

        // Configurar evento de sucesso
        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    if (onSuccess) onSuccess(response);
                    resolve(response);
                } catch (e) {
                    const error = new Error('Erro ao processar resposta do servidor');
                    if (onError) onError(error);
                    reject(error);
                }
            } else {
                try {
                    const errorResponse = JSON.parse(xhr.responseText);
                    const error = new Error(errorResponse.title || `Erro ${xhr.status}: ${xhr.statusText}`);
                    if (onError) onError(error);
                    reject(error);
                } catch (e) {
                    const error = new Error(`Erro ${xhr.status}: ${xhr.statusText}`);
                    if (onError) onError(error);
                    reject(error);
                }
            }
        });

        // Configurar evento de erro
        xhr.addEventListener('error', () => {
            const error = new Error('Erro de rede ao fazer upload');
            if (onError) onError(error);
            reject(error);
        });

        // Configurar evento de cancelamento
        xhr.addEventListener('abort', () => {
            const error = new Error('Upload cancelado');
            if (onError) onError(error);
            reject(error);
        });

        // Configurar e enviar requisição
        xhr.open('POST', apiUrl);
        xhr.setRequestHeader('Authorization', `Bearer ${authToken}`);
        xhr.send(formData);
    });
}

// Exemplo de uso:
/*
// Obter token (do localStorage, contexto, etc)
const token = localStorage.getItem('authToken');

// Obter arquivo do input
const fileInput = document.getElementById('fileInput');
const file = fileInput.files[0];

// Fazer upload
uploadFile(
    file,
    'http://localhost:5000/v1/files/upload',
    token,
    // onProgress
    (progress) => {
        console.log(`Progresso: ${progress}%`);
        // Atualizar barra de progresso
        document.getElementById('progressBar').style.width = progress + '%';
    },
    // onSuccess
    (response) => {
        console.log('Upload concluído:', response);
        console.log('URL do arquivo:', response.data.url);
        console.log('ID do arquivo:', response.data.fileId);
    },
    // onError
    (error) => {
        console.error('Erro no upload:', error.message);
        alert('Erro ao fazer upload: ' + error.message);
    }
);
*/

// Versão usando async/await:
/*
async function uploadFileAsync(file, authToken) {
    try {
        const response = await uploadFile(
            file,
            'http://localhost:5000/v1/files/upload',
            authToken,
            (progress) => {
                console.log(`Progresso: ${progress}%`);
            }
        );
        console.log('Upload concluído:', response);
        return response;
    } catch (error) {
        console.error('Erro:', error);
        throw error;
    }
}

// Uso:
const file = document.getElementById('fileInput').files[0];
const token = localStorage.getItem('authToken');
const result = await uploadFileAsync(file, token);
*/
