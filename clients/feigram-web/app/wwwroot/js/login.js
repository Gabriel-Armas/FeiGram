localStorage.setItem('jwtToken', token);

fetch('/feed', {
  headers: {
    'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
  }
});
