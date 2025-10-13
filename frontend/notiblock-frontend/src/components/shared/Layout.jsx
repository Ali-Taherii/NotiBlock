import HeaderBar from './HeaderBar'
import SidebarNav from './SidebarNav'
import Notifications from './Notifications'
import ActivityFeed from './ActivityFeed'

export default function Layout({ role, children }) {
  return (
    <div className="flex min-h-screen">
      <SidebarNav role={role} />
      <div className="flex-1">
        <HeaderBar />
        <div className="p-6">{children}</div>
        <ActivityFeed />
        <Notifications />
      </div>
    </div>
  )
}
