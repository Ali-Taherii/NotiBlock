export default function SidebarNav({ role }) {
  const links = {
    manufacturer: [
      { name: 'Issue Recall', href: '/manufacturer/dashboard' },
      { name: 'My Recalls', href: '/manufacturer/recalls' },
    ],
    consumer: [
      { name: 'My Products', href: '/consumer/dashboard' },
      { name: 'Active Recalls', href: '/consumer/recalls' },
    ],
    regulator: [
      { name: 'Analytics', href: '/regulator/dashboard' },
      { name: 'Audit Trail', href: '/regulator/audit' },
    ]
  }

  return (
    <div className="w-64 bg-gray-800 text-white p-4">
      <h2 className="text-xl font-bold mb-4">NotiBlock</h2>
      <ul className="space-y-2">
        {links[role]?.map(link => (
          <li key={link.name}>
            <a href={link.href} className="block hover:bg-gray-700 p-2 rounded">
              {link.name}
            </a>
          </li>
        ))}
      </ul>
    </div>
  )
}
