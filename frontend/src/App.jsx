import { Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import Home from './pages/guest/Home'
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'
import './App.css'

function App() {
  return (
    <>
      <Header />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
      </Routes>
    </>
  )
}

export default App
