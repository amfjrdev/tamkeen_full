export interface TransactionStat {
  label: string;
  value: string;
}

export type TransactionStatus = 'completed' | 'pending' | 'failed';

export interface Transaction {
  id: string;
  provider: string;
  package: string;
  credits: number;
  amount: number;
  method: string;
  status: TransactionStatus;
  date: string;
}

export interface TransactionActivity {
  id: number;
  user: string;
  package: string;
  credits: number;
  amount: number;
  status: TransactionStatus;
  time: string;
}

export interface TransactionsData {
  stats: TransactionStat[];
  transactions: Transaction[];
  activity: TransactionActivity[];
  navItems: Array<{
    label: string;
    icon: string;
    active: boolean;
  }>;
}
