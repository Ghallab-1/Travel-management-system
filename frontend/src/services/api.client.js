import Api from './api';

const Client = {
  login: Api.login,
  getTravelRequests: Api.getTravelRequests,
  getMyRequests: Api.getMyRequests,
  getNotifications: Api.getNotifications,
};

export default Client;
