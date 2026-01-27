// Versão simples - apenas a função essencial
function uploadFile(file, token, onProgress) {
    return new Promise((resolve, reject) => {
        const formData = new FormData();
        formData.append('file', file);

        const xhr = new XMLHttpRequest();

        // Progresso
        xhr.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable && onProgress) {
                const percent = Math.round((e.loaded / e.total) * 100);
                onProgress(percent);
            }
        });

        // Sucesso
        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(JSON.parse(xhr.responseText));
            } else {
                reject(new Error(`Erro ${xhr.status}`));
            }
        });

        // Erro
        xhr.addEventListener('error', () => reject(new Error('Erro de rede')));

        // Enviar
        xhr.open('POST', 'http://localhost:5000/v1/files/upload');
        xhr.setRequestHeader('Authorization', `Bearer ${token}`);
        xhr.send(formData);
    });
}

// Uso básico:
// const file = document.getElementById('fileInput').files[0];
// const token = localStorage.getItem('authToken');
// 
// uploadFile(file, token, (progress) => {
//     console.log(progress + '%');
// }).then(response => {
//     console.log('Sucesso:', response);
// }).catch(error => {
//     console.error('Erro:', error);
// });
