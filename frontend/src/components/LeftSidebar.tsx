'use client';

import Link from 'next/link';
import { usePathname } from 'next/navigation';
import { useAuthStore } from '@/store/authStore';

interface NavItemProps {
  href: string;
  icon: string;
  label: string;
  active?: boolean;
}

function NavItem({ href, icon, label, active }: NavItemProps) {
  return (
    <Link
      href={href}
      className={`flex items-center gap-4 rounded-full px-4 py-3 transition-colors hover:bg-gray-800 ${
        active ? 'font-bold' : ''
      }`}
    >
      <span className="text-2xl">{icon}</span>
      <span className="hidden text-xl xl:inline">{label}</span>
    </Link>
  );
}

export default function LeftSidebar() {
  const pathname = usePathname();
  const { user } = useAuthStore();

  return (
    <div className="flex h-screen flex-col justify-between py-4">
      <div className="flex flex-col gap-2">
        {/* Logo */}
        <Link href="/dashboard" className="mb-4 px-4 text-3xl hover:bg-gray-800 rounded-full p-3 w-fit">
          🐦
        </Link>

        {/* Navigation */}
        <NavItem
          href="/dashboard"
          icon="🏠"
          label="Inicio"
          active={pathname === '/dashboard'}
        />
        
        {user?.username && (
          <NavItem
            href={`/user/${user.username}`}
            icon="👤"
            label="Perfil"
            active={pathname?.includes(`/user/${user.username}`)}
          />
        )}
      </div>

      {/* User Info */}
      {user && user.name && user.username && (
        <div className="mt-auto px-4 py-3 hover:bg-gray-800 rounded-full cursor-pointer flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-blue-500 text-white font-bold">
            {user.name.charAt(0).toUpperCase()}
          </div>
          <div className="hidden xl:block">
            <div className="font-bold text-sm">{user.name}</div>
            <div className="text-gray-500 text-sm">@{user.username}</div>
          </div>
        </div>
      )}
    </div>
  );
}
