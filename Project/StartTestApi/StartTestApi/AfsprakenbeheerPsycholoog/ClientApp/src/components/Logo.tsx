import React from 'react';

interface LogoProps {
  className?: string;
  variant?: 'primary' | 'secundair';
}

export const Logo: React.FC<LogoProps> = ({ className = "h-10 w-10", variant = 'primary' }) => {
  const isDark = variant === 'secundair';
  
  return (
    <svg 
      viewBox="0 0 120 80" 
      fill="none" 
      xmlns="http://www.w3.org/2000/svg"
      className={`${className} transition-transform duration-300`}
    >
      {/* Blue-Grey Bubble (Couch Frame) */}
      <g fill={isDark ? "#ffffff" : "#7292b2"}>
        <rect x="20" y="30" width="60" height="30" rx="10" />
        <path d="M28 58 L28 72 C31 67 36 62 39 59 Z" />
      </g>
      {/* Old Pink Bubble (Couch Backrest) */}
      <g fill={isDark ? "#ffffff" : "#e89c9b"} opacity={isDark ? "0.85" : "1"}>
        <rect x="28" y="18" width="48" height="26" rx="8" />
        <path d="M68 43 C70 48 74 58 75 64 L72 43 Z" />
      </g>
    </svg>
  );
};
