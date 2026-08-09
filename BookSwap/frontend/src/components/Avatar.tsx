export function Avatar({ name, url, size = 42 }: { name: string; url?: string; size?: number }) {
  const initials = name.split(' ').slice(0, 2).map(part => part[0]).join('').toUpperCase()
  return url
    ? <img className="avatar" src={url} alt={name} style={{ width: size, height: size }} />
    : <div className="avatar avatar-fallback" style={{ width: size, height: size, fontSize: size * .34 }}>{initials}</div>
}
