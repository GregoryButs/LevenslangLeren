import axios from 'axios';
import { 
  User, Patient, Afspraak, AfspraakType, 
  DashboardData, WeekOverzicht, DagOverzicht 
} from '../types';

const api = axios.create({
  baseURL: '/api',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  }
});

// Response interceptor to handle 401 globally
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response && error.response.status === 401) {
      // Clear local storage and redirect to login if not on public pages (/ or login/register)
      if (window.location.pathname !== '/' && !window.location.pathname.includes('/login') && !window.location.pathname.includes('/register')) {
        localStorage.removeItem('user');
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

export const authApi = {
  login: async (email: string, password: string, rememberMe = false): Promise<User> => {
    const res = await api.post<User>('/auth/login', { email, password, rememberMe });
    return res.data;
  },
  register: async (voornaam: string, achternaam: string, email: string, password: string): Promise<User> => {
    const res = await api.post<User>('/auth/register', { voornaam, achternaam, email, password });
    return res.data;
  },
  logout: async (): Promise<void> => {
    await api.post('/auth/logout');
  },
  me: async (): Promise<User> => {
    const res = await api.get<User>('/auth/me');
    return res.data;
  },
  resendConfirmation: async (email: string): Promise<void> => {
    await api.post('/auth/resend-confirmation', { email });
  },
  forgotPassword: async (email: string): Promise<{ message: string }> => {
    const res = await api.post<{ message: string }>('/auth/forgot-password', { email });
    return res.data;
  },
  resetPassword: async (email: string, token: string, newPassword: string): Promise<{ message: string }> => {
    const res = await api.post<{ message: string }>('/auth/reset-password', { email, token, newPassword });
    return res.data;
  }
};

export const dashboardApi = {
  getDashboard: async (weekDatum?: string): Promise<DashboardData> => {
    const res = await api.get<DashboardData>('/dashboard', {
      params: weekDatum ? { weekDatum } : {}
    });
    return res.data;
  },
  getWeekOverzicht: async (datum: string, patientId?: number, isPsycholoog = false): Promise<WeekOverzicht> => {
    const res = await api.get<WeekOverzicht>('/dashboard/weekoverzicht', {
      params: { datum, patientId, isPsycholoog }
    });
    return res.data;
  }
};

export const patientApi = {
  getAll: async (): Promise<Patient[]> => {
    const res = await api.get<Patient[]>('/patient');
    return res.data;
  },
  getArchive: async (): Promise<Patient[]> => {
    const res = await api.get<Patient[]>('/patient/archief');
    return res.data;
  },
  getById: async (id: number): Promise<Patient> => {
    const res = await api.get<Patient>(`/patient/${id}`);
    return res.data;
  },
  create: async (patient: Omit<Patient, 'id' | 'volledigeNaam' | 'isActief'>): Promise<{ id: number }> => {
    const res = await api.post<{ id: number }>('/patient', patient);
    return res.data;
  },
  update: async (id: number, patient: Patient): Promise<void> => {
    await api.put(`/patient/${id}`, patient);
  },
  delete: async (id: number): Promise<void> => {
    await api.delete(`/patient/${id}`);
  },
  reactivate: async (id: number): Promise<void> => {
    await api.post(`/patient/${id}/heractiveer`);
  },
  getAanmeldingen: async (): Promise<any[]> => {
    const res = await api.get<any[]>('/patient/aanmeldingen');
    return res.data;
  },
  approveAanmelding: async (userId: string): Promise<void> => {
    await api.post(`/patient/aanmeldingen/${userId}/goedkeuren`);
  },
  link: async (patientId: number, email: string): Promise<void> => {
    await api.post('/patient/koppel', { patientId, email });
  },
  unlink: async (patientId: number): Promise<void> => {
    await api.post(`/patient/${patientId}/ontkoppel`);
  }
};

export const afspraakApi = {
  getAll: async (): Promise<Afspraak[]> => {
    const res = await api.get<Afspraak[]>('/afspraak');
    return res.data;
  },
  getDagplanning: async (datum?: string): Promise<{ datum: string; tijdsloten: TijdslotData[] }> => {
    const res = await api.get<{ datum: string; tijdsloten: TijdslotData[] }>('/afspraak/dagplanning', {
      params: datum ? { datum } : {}
    });
    return res.data;
  },
  getById: async (id: number): Promise<Afspraak> => {
    const res = await api.get<Afspraak>(`/afspraak/${id}`);
    return res.data;
  },
  getCreateData: async (): Promise<{ patienten: { id: number; naam: string }[]; types: AfspraakType[] }> => {
    const res = await api.get<{ patienten: { id: number; naam: string }[]; types: AfspraakType[] }>('/afspraak/create-data');
    return res.data;
  },
  create: async (afspraak: any): Promise<void> => {
    await api.post('/afspraak', afspraak);
  },
  getEditData: async (id: number): Promise<{ viewModel: any; patienten: { id: number; naam: string }[]; types: AfspraakType[] }> => {
    const res = await api.get<{ viewModel: any; patienten: { id: number; naam: string }[]; types: AfspraakType[] }>(`/afspraak/${id}/edit-data`);
    return res.data;
  },
  update: async (id: number, afspraak: any): Promise<void> => {
    await api.put(`/afspraak/${id}`, afspraak);
  },
  delete: async (id: number): Promise<void> => {
    await api.delete(`/afspraak/${id}`);
  },
  deleteReeks: async (reeksId: string): Promise<void> => {
    await api.delete(`/afspraak/reeks/${reeksId}`);
  }
};

export interface TijdslotData {
  label: string;
  starttijd: string;
  eindtijd: string;
  isBezet: boolean;
  isBlokkering: boolean;
  afspraak: Afspraak | null;
}

export const patientPortaalApi = {
  getMijnAfspraken: async (): Promise<Afspraak[]> => {
    const res = await api.get<Afspraak[]>('/patientportaal/mijnafspraken');
    return res.data;
  },
  getBoeken: async (datum?: string): Promise<{ dagOverzicht: DagOverzicht; viewModel: any }> => {
    const res = await api.get<{ dagOverzicht: DagOverzicht; viewModel: any }>('/patientportaal/boeken', {
      params: datum ? { datum } : {}
    });
    return res.data;
  },
  book: async (booking: any): Promise<void> => {
    await api.post('/patientportaal/boeken', booking);
  },
  cancel: async (id: number): Promise<void> => {
    await api.post(`/patientportaal/annuleren/${id}`);
  }
};

export const settingsApi = {
  get: async (): Promise<any> => {
    const res = await api.get('/settings');
    return res.data;
  },
  update: async (settings: any): Promise<any> => {
    const res = await api.post('/settings', settings);
    return res.data;
  },
  syncCalendar: async (): Promise<{ message: string }> => {
    const res = await api.post<{ message: string }>('/settings/sync-calendar');
    return res.data;
  },
  cleanResync: async (): Promise<{ message: string }> => {
    const res = await api.post<{ message: string }>('/settings/clean-resync');
    return res.data;
  }
};

export const afspraakTypeApi = {
  getAll: async (): Promise<AfspraakType[]> => {
    const res = await api.get<AfspraakType[]>('/afspraaktype');
    return res.data;
  },
  getById: async (id: number): Promise<AfspraakType> => {
    const res = await api.get<AfspraakType>(`/afspraaktype/${id}`);
    return res.data;
  },
  create: async (type: AfspraakType): Promise<AfspraakType> => {
    const res = await api.post<AfspraakType>('/afspraaktype', type);
    return res.data;
  },
  update: async (id: number, type: AfspraakType): Promise<void> => {
    await api.put(`/afspraaktype/${id}`, type);
  },
  delete: async (id: number): Promise<void> => {
    await api.delete(`/afspraaktype/${id}`);
  }
};

export const aiApi = {
  getPatientRisks: async (): Promise<any[]> => {
    const res = await api.get<any[]>('/ai/patient-risks');
    return res.data;
  },
  getSyntheticPatients: async (page: number, pageSize: number, search = ''): Promise<{ patients: any[]; totalCount: number; page: number; pageSize: number }> => {
    const res = await api.get<{ patients: any[]; totalCount: number; page: number; pageSize: number }>('/ai/synthetic-patients', {
      params: { page, pageSize, search }
    });
    return res.data;
  },
  simulateStep: async (sessionsCompleted: number, stability: number, gap: number, sentiment: number, sentimentEma: number, action: number): Promise<any> => {
    const res = await api.post('/ai/simulator/step', {
      sessionsCompleted,
      stability,
      gap,
      sentiment,
      sentimentEma,
      action
    });
    return res.data;
  },
  getHeatmapUrl: (): string => {
    return '/api/ai/heatmap';
  }
};

export const contactApi = {
  send: async (data: { name: string; surname: string; email: string; message: string }): Promise<void> => {
    await api.post('/contact', data);
  }
};

export default api;
