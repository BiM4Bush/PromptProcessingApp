import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { fetchPrompts, createPrompt } from './api';

function App() {
  const [inputValue, setInputValue] = useState('');
  const queryClient = useQueryClient();


  const { data: prompts, isLoading, isError } = useQuery({
    queryKey: ['prompts'],
    queryFn: fetchPrompts,
    refetchInterval: 3000, 
  });

  const mutation = useMutation({
    mutationFn: createPrompt,
    onSuccess: () => {
      setInputValue('');
      queryClient.invalidateQueries({ queryKey: ['prompts'] });
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!inputValue.trim()) return;
    mutation.mutate(inputValue);
  };

  const getStatusBadgeClasses = (status: string) => {
    switch (status) {
      case 'Pending':
        return 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/30';
      case 'Processing':
        return 'bg-blue-500/20 text-blue-400 border border-blue-500/30 animate-pulse';
      case 'Completed':
        return 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30';
      case 'Failed':
        return 'bg-red-500/20 text-red-400 border border-red-500/30';
      default:
        return 'bg-slate-700 text-slate-300 border border-slate-600';
  }
};
  return (
    <div className="min-h-screen bg-slate-900 text-slate-200 p-8">
      <div className="max-w-3xl mx-auto space-y-8">
        
        <div className="bg-slate-800 p-6 rounded-xl shadow-lg border border-slate-700">
          <h1 className="text-2xl font-bold text-white mb-4">AI Prompt Processor</h1>
          <form onSubmit={handleSubmit} className="flex gap-4">
            <input
              type="text"
              value={inputValue}
              onChange={(e) => setInputValue(e.target.value)}
              disabled={mutation.isPending}
              placeholder="Wpisz swój prompt (np. 'Opowiedz dowcip o programistach')..."
              className="flex-1 px-4 py-2 bg-slate-900 border border-slate-600 rounded-lg focus:outline-none focus:border-emerald-500 text-white placeholder-slate-400"
            />
            <button
              type="submit"
              disabled={mutation.isPending || !inputValue.trim()}
              className="px-6 py-2 bg-emerald-600 hover:bg-emerald-500 text-white font-semibold rounded-lg transition-colors disabled:opacity-50"
            >
              {mutation.isPending ? 'Wysyłanie...' : 'Wyślij'}
            </button>
          </form>
        </div>

        <div className="space-y-4">
          <h2 className="text-xl font-semibold text-white">Historia zadań</h2>
          
          {isLoading && <p className="text-slate-400">Ładowanie danych...</p>}
          {isError && <p className="text-red-400">Nie udało się połączyć z serwerem.</p>}

          {prompts?.map((prompt) => (
            <div key={prompt.id} className="bg-slate-800 p-4 rounded-lg border border-slate-700 flex flex-col gap-2">
              <div className="flex justify-between items-start">
                <span className="font-medium text-emerald-400">{prompt.promptContent}</span>
                <span className={`text-xs font-semibold px-2.5 py-1 rounded-full ${getStatusBadgeClasses(prompt.status)}`}>
                  {prompt.status.toUpperCase()}
                </span>
              </div>
              
              <div className="text-slate-400 text-sm bg-slate-900 p-3 rounded mt-2 border border-slate-800">
                {prompt.status === 'Completed' && prompt.result && (
                  <p className="text-slate-200">{prompt.result}</p>
                )}
                {prompt.status === 'Failed' && (
                  <p className="text-red-400">Wystąpił błąd podczas przetwarzania.</p>
                )}
                {(prompt.status === 'Pending' || prompt.status === 'Processing') && (
                  <p className="animate-pulse">AI przetwarza Twoje zapytanie...</p>
                )}
              </div>
            </div>
          ))}
          
          {prompts?.length === 0 && !isLoading && (
            <p className="text-slate-500 text-center py-8">Brak zadań. Wyślij swój pierwszy prompt!</p>
          )}
        </div>

      </div>
    </div>
  );
}

export default App;