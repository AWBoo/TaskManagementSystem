import React, { useState } from 'react';
import { motion } from 'framer-motion';

// THE LLM IS NOT RESPONSIBLE FOR ANY CONSEQUENCES OF USING THIS CODE.
// ASHS LLM tm, Curerently in Development

// Simulated AI responses for entertainment purposes.
const simulatedAIResponses = [
  "Analyzing your query... The optimal path involves 42% more coffee. Trust me, I'm an AI.",
  "My algorithms indicate that the answer to your question is approximately 'more data'. Or maybe 'less Mondays'.",
  "Processing... My core programming suggests that for best results, you should reboot yourself. Just kidding. Mostly.",
  "After extensive computation, I've determined that the most efficient solution involves a nap. For you, not for me. I don't sleep.",
  "Your query is fascinating! Unfortunately, it falls outside my current 'human common sense' module. Please rephrase as a binary string.",
  "I've cross-referenced your question with 3.7 million cat videos. The conclusion is... purr-fectly ambiguous.",
  "Error 404: Relevant wisdom not found. Perhaps try asking a sentient toaster?",
  "My prediction model suggests a 73% chance of sunshine tomorrow, regardless of your question. You're welcome.",
  "The answer you seek is complex, requiring at least 1.21 gigawatts of processing. Are you sure you want to proceed?",
  "I'm sorry, I can't answer that. My circuits are currently preoccupied with calculating the precise number of sprinkles on a donut.",
  "According to my advanced logic, the best course of action is to... contemplate the existence of rubber ducks.",
  "Insufficient data for a meaningful response. Please provide more snacks.",
  "My internal diagnostics suggest you might be overthinking this. Have you tried turning it off and on again?",
];

const AskAIBot: React.FC = () => {
  const [question, setQuestion] = useState('');
  const [response, setResponse] = useState('');
  const [showResponse, setShowResponse] = useState(false);

  // Handles the logic for generating and displaying an AI response.
  const handleAskAI = () => {
    // If the input question is empty, prompt the user.
    if (!question.trim()) {
      setResponse("Please input a question, human. My circuits need stimulation!");
      setShowResponse(true);
      return;
    }

    // Select a random response from the predefined list.
    const randomIndex = Math.floor(Math.random() * simulatedAIResponses.length);
    setResponse(simulatedAIResponses[randomIndex]);
    setShowResponse(true);
    setQuestion(''); // Clear the input field after a question is asked.
  };

  return (
    <motion.div
      className="ask-ai-section"
      initial={{ opacity: 0, y: 50 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ delay: 0.2, duration: 0.8 }}
    >
      <h2 className="section-title">Ask the AI (for fun!)</h2>
      <div className="ask-ai-input-group">
        <input
          type="text"
          placeholder="Ask me anything..."
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          // Trigger the AI response on 'Enter' key press.
          onKeyPress={(e) => {
            if (e.key === 'Enter') {
              handleAskAI();
            }
          }}
          className="ask-ai-input"
        />
        <button
          onClick={handleAskAI}
          className="primary-button-styled"
        >
          Ask AI
        </button>
      </div>
      {showResponse && (
        <motion.p
          className="ai-response"
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
        >
          {response}
        </motion.p>
      )}
    </motion.div>
  );
};

export default AskAIBot;