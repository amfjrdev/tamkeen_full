export interface MessagingStat {
  id: number;
  label: string;
  value: string;
  trend: string;
  icon: 'chat' | 'message' | 'alert' | 'lock';
  color: 'green' | 'blue' | 'red' | 'orange';
}

export type ConversationStatus = 'active' | 'reported';

export interface Conversation {
  id: number;
  participant1: string;
  participant2: string;
  lastMessage: string;
  time: string;
  messageCount: number;
  status: ConversationStatus;
  isLocked: boolean;
}

export interface ReportedMessage {
  id: number;
  participant1: string;
  participant2: string;
  time: string;
  content: string;
  reportedBy: string;
  reason: string;
}

export interface MessagingData {
  stats: MessagingStat[];
  conversations: Conversation[];
  reportedMessages: ReportedMessage[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
