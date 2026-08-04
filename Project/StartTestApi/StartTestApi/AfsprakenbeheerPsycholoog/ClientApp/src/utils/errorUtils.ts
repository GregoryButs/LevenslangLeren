/**
 * Utility functies voor consistente afhandeling van API-foutmeldingen en netwerkfouten.
 */

export function extractErrorMessage(error: unknown, fallbackMessage = 'Er is een onverwachte fout opgetreden.'): string {
  if (!error) return fallbackMessage;

  if (typeof error === 'string') return error;

  if (typeof error === 'object' && error !== null) {
    const err = error as Record<string, any>;

    if (err.response?.data) {
      const data = err.response.data;
      if (typeof data === 'string') return data;
      if (data.detail) return data.detail;
      if (data.message) return data.message;
      if (data.errors && typeof data.errors === 'object') {
        const messages: string[] = [];
        for (const [field, msgs] of Object.entries(data.errors)) {
          if (Array.isArray(msgs)) {
            messages.push(`${field}: ${msgs.join(', ')}`);
          } else if (typeof msgs === 'string') {
            messages.push(`${field}: ${msgs}`);
          }
        }
        if (messages.length > 0) {
          return messages.join(' | ');
        }
      }
      if (data.title) return data.title;
    }

    if (err.message && typeof err.message === 'string') {
      return err.message;
    }
  }

  return fallbackMessage;
}

