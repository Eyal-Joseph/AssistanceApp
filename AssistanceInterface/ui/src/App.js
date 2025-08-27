
import './App.css';
import { useState } from 'react';

function App() {
  const [messages, setMessages] = useState([
    { sender: 'ai', text: 'Hello! How can I help you today?' }
  ]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);

  const sendMessage = async (e) => {
    e.preventDefault();
    if (!input.trim()) return;
    const userMsg = { sender: 'user', text: input };
    setMessages((msgs) => [...msgs, userMsg]);
    setInput('');
    setLoading(true);
    try {
      // Replace the URL below with your actual C# AI agent API endpoint
      const res = await fetch('http://localhost:5000/api/chat', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: input })
      });
      const data = await res.json();
      setMessages((msgs) => [...msgs, { sender: 'ai', text: data.reply }]);
    } catch (err) {
      setMessages((msgs) => [...msgs, { sender: 'ai', text: 'Sorry, there was an error connecting to the AI agent.' }]);
    }
    setLoading(false);
  };

  return (
    <div className="App" style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', minHeight: '100vh', background: '#282c34' }}>
      <h2 style={{ color: '#fff', marginTop: 40 }}>AI Chat Interface</h2>
      <div style={{ background: '#fff', borderRadius: 8, width: 400, minHeight: 400, margin: '20px 0', padding: 16, display: 'flex', flexDirection: 'column', boxShadow: '0 2px 8px rgba(0,0,0,0.1)' }}>
        <div style={{ flex: 1, overflowY: 'auto', marginBottom: 12 }}>
          {messages.map((msg, idx) => (
            <div key={idx} style={{ textAlign: msg.sender === 'user' ? 'right' : 'left', margin: '8px 0' }}>
              <span style={{
                display: 'inline-block',
                background: msg.sender === 'user' ? '#61dafb' : '#eee',
                color: '#222',
                borderRadius: 16,
                padding: '8px 16px',
                maxWidth: '80%',
                wordBreak: 'break-word'
              }}>{msg.text}</span>
            </div>
          ))}
          {loading && <div style={{ color: '#888', fontStyle: 'italic' }}>AI is typing...</div>}
        </div>
        <form onSubmit={sendMessage} style={{ display: 'flex' }}>
          <input
            type="text"
            value={input}
            onChange={e => setInput(e.target.value)}
            placeholder="Type your message..."
            style={{ flex: 1, padding: 10, borderRadius: 16, border: '1px solid #ccc', marginRight: 8 }}
            disabled={loading}
            autoFocus
          />
          <button type="submit" disabled={loading || !input.trim()} style={{ padding: '0 20px', borderRadius: 16, border: 'none', background: '#61dafb', color: '#222', fontWeight: 'bold' }}>Send</button>
        </form>
      </div>
    </div>
  );
}

export default App;
