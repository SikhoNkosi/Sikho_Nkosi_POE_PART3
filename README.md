# Sikho_Nkosi_POE_PART3

Overview
In Part 3, the chatbot was upgraded to be smarter, more interactive, and more transparent. This stage focused on data persistence, natural language processing (NLP), and user activity tracking, while also adding a cybersecurity quiz feature.

Features Implemented
1. JSON Storage

Implemented full CRUD operations:

Add Task

View Tasks

Update Reminder

Complete Task

Delete Task

Ensures tasks are saved and loaded even after restarting the chatbot.

2. NLP Keyword Detection
Integrated keyword dictionaries to interpret user input flexibly.

Recognizes commands even when phrased differently:

Add a task

Remind me to update my password

Show my tasks

Complete task 1

Delete task 2

Reduced reliance on “I didn’t understand” by mapping synonyms and variations.

3. Activity Log
Added an activity log to track recent chatbot actions.

Logs include:

Tasks added, updated, completed, or deleted

Reminders set

Quiz attempts

NLP‑interpreted commands

User can type “Show activity log” or “What have you done for me?” to view the last 5–10 actions.

Provides transparency and accountability.

4. Cybersecurity Quiz
Created a small quiz feature where users can test themselves on cybersecurity knowledge.

Multiple-choice questions on topics like:

Password safety

Phishing awareness

Privacy settings

Tracks quiz attempts in the activity log.

Helps users learn while interacting with the chatbot.

5. Improved Chatbot Responses
Dynamic confirmations for actions:

Task added successfully.

Task completed.

Task deleted.

Clear task listings when requested.

Retained earlier Part 1 & 2 features:

Sentiment detection

Motivational quotes

Jokes
