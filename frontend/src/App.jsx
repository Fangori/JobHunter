import { Route, Routes } from 'react-router-dom'
import Header from './components/Header'
import ProtectedRoute from './components/ProtectedRoute'
import Home from './pages/guest/Home'
import JobDetail from './pages/guest/JobDetail'
import CompanyDetail from './pages/guest/CompanyDetail'
import Login from './pages/auth/Login'
import Register from './pages/auth/Register'
import VerifyEmail from './pages/auth/VerifyEmail'
import ForgotPassword from './pages/auth/ForgotPassword'
import ResetPassword from './pages/auth/ResetPassword'
import PostJob from './pages/employer/PostJob'
import Applicants from './pages/employer/Applicants'
import CompanyProfile from './pages/employer/CompanyProfile'
import MyJobs from './pages/employer/MyJobs'
import ApproveJobs from './pages/admin/ApproveJobs'
import ManageCv from './pages/candidate/ManageCv'
import Profile from './pages/candidate/Profile'
import MyApplications from './pages/candidate/MyApplications'
import Favorites from './pages/candidate/Favorites'
import './App.css'

function App() {
  return (
    <>
      <Header />
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/jobs/:id" element={<JobDetail />} />
        <Route path="/companies/:id" element={<CompanyDetail />} />
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/verify-email" element={<VerifyEmail />} />
        <Route path="/forgot-password" element={<ForgotPassword />} />
        <Route path="/reset-password" element={<ResetPassword />} />
        <Route path="/employer/post-job" element={
          <ProtectedRoute role="NhaTuyenDung"><PostJob /></ProtectedRoute>
        } />
        <Route path="/employer/jobs/:id/edit" element={
          <ProtectedRoute role="NhaTuyenDung"><PostJob /></ProtectedRoute>
        } />
        <Route path="/employer/jobs/:id/applicants" element={
          <ProtectedRoute role="NhaTuyenDung"><Applicants /></ProtectedRoute>
        } />
        <Route path="/employer/my-jobs" element={
          <ProtectedRoute role="NhaTuyenDung"><MyJobs /></ProtectedRoute>
        } />
        <Route path="/employer/profile" element={
          <ProtectedRoute role="NhaTuyenDung"><CompanyProfile /></ProtectedRoute>
        } />
        <Route path="/admin/pending-jobs" element={
          <ProtectedRoute role="Admin"><ApproveJobs /></ProtectedRoute>
        } />
        <Route path="/candidate/cvs" element={
          <ProtectedRoute role="UngVien"><ManageCv /></ProtectedRoute>
        } />
        <Route path="/candidate/profile" element={
          <ProtectedRoute role="UngVien"><Profile /></ProtectedRoute>
        } />
        <Route path="/candidate/applications" element={
          <ProtectedRoute role="UngVien"><MyApplications /></ProtectedRoute>
        } />
        <Route path="/candidate/favorites" element={
          <ProtectedRoute role="UngVien"><Favorites /></ProtectedRoute>
        } />
      </Routes>
    </>
  )
}

export default App
