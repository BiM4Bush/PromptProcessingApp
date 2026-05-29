export interface PromptResponse {
  id: string;
  promptContent: string;
  result: string | null;
  status: string; // 'Pending', 'Processing', 'Completed', 'Failed'
  createdAt: string;
}

const API_BASE_URL = 'http://localhost:5168/api/prompts'; 

export const fetchPrompts = async (): Promise<PromptResponse[]> => {
  const response = await fetch(API_BASE_URL);
  if (!response.ok) throw new Error('Błąd pobierania promptów');
  return response.json();
};

export const createPrompt = async (promptContent: string): Promise<PromptResponse> => {
  const response = await fetch(API_BASE_URL, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ promptContent }),
  });
  if (!response.ok) throw new Error('Błąd tworzenia promptu');
  return response.json();
};