import { Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import ProtectedRoute from './components/ProtectedRoute'
import Home from './pages/guest/Home'
import JobDetail from './pages/guest/JobDetail'
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'
import PostJob from './pages/employer/PostJob'
import ApproveJobs from './pages/admin/ApproveJobs'
import './App.css'

function App() {
  return (
    <>
      <Header />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/jobs/:id" element={<JobDetail />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/employer/post-job" element={
          <ProtectedRoute role="NhaTuyenDung"><PostJob /></ProtectedRoute>
        } />
        <Route path="/admin/pending-jobs" element={
          <ProtectedRoute role="Admin"><ApproveJobs /></ProtectedRoute>
        } />
      </Routes>
    </>
  )
}

export default App
