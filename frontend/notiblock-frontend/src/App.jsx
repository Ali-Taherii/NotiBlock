import AppRouter from "./router/index.jsx";
import { AuthProvider } from "./contexts/AuthProvider.jsx";
import "./styles/App.css";
import HeaderBar from "./components/shared/HeaderBar.jsx";

export default function App() {
  return (
    <AuthProvider>
      <HeaderBar/>
      <AppRouter />
    </AuthProvider>
  );
}
